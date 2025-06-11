using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TezGel.Application.DTOs.RabbitMq;
using TezGel.Application.Expections;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Application.Interfaces.Services;
using TezGel.Domain.Entities;
using TezGel.Domain.Enums;

namespace TezGel.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ILockService _lockService;
        private readonly IProductRepository _productRepo;
        private readonly IActionRepository _resRepo;
        private readonly IMessagePublisher _messagePublisher; 
        private readonly ILogger<ReservationService> _logger;


        public ReservationService(
            ILockService lockService,
            IProductRepository productRepo,
            IActionRepository resRepo,IMessagePublisher messagePublisher,
            ILogger<ReservationService> logger)
        {
            _lockService = lockService;
            _productRepo = productRepo;
            _resRepo = resRepo;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        public async Task<ActionReservation> ReserveProductAsync(Guid userId, Guid productId) 
        {
           // _logger.LogInformation("Attempting to reserve product {ProductId} for user {UserId}", productId, userId);

            // 1) Ürün var mı, aktif mi, tarihi geçmiş mi?
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null || !product.IsActive || product.ExpireAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Product {ProductId} cannot be reserved: Not found, inactive, or expired.", productId);
                throw new InvalidOperationException("Ürün rezerve edilemez.");
            }

            // 2) Redis lock (Bu kısım önemli, RabbitMQ mesajı lock alındıktan SONRA gönderilmeli)
            var lockKey = $"product:lock:{productId}";
            // Sizin mevcut kodunuzda 10dk lock alıyorsunuz, RabbitMQ TTL'i ile tutarlı olmalı.
            // RabbitMQ TTL'i 10 dakika, ActionReservation ExpireAt 1 dakika. Bu tutarsızlık var, dikkat!
            // Bizim amacımız ActionReservation'ın 10dk sonra expire olması ise, ExpireAt'i 10dk yapmalıyız.
            // RabbitMQ'ya gönderdiğimiz mesajın amacı bu süreyi takip etmek.
            TimeSpan reservationDuration = TimeSpan.FromMinutes(10); // Örnek: RabbitMQ TTL ile aynı

            var locked = await _lockService.TryAcquireLockAsync(lockKey, reservationDuration);
            if (!locked)
            {
                _logger.LogWarning("Failed to acquire lock for product {ProductId}. Already locked.", productId);
                throw new InvalidOperationException("Ürün başka bir kullanıcıya zaten rezerve edilmiş veya rezerve aşamasında.");
            }
            _logger.LogInformation("Lock acquired for product {ProductId}", productId);

            // 3) Rezervasyon Kaydı Oluşturma
            var reservation = new ActionReservation
            {
                // Id alanı otomatik oluşuyorsa (örn: Guid.NewGuid() veya DB identity)
                // veya AddAsync sonrası set ediliyorsa onu kullanacağız.
                // Şimdilik AddAsync öncesi bir Id atadığımızı varsayalım.
                Id = Guid.NewGuid(), // Bu ID'yi RabbitMQ mesajında kullanacağız
                UserId = userId,
                ProductId = productId,
                ExpireAt = DateTime.UtcNow.Add(reservationDuration), // ÖNEMLİ: RabbitMQ TTL ile tutarlı olmalı
                Status = ActionStatus.Pending
            };
            await _resRepo.AddAsync(reservation); // AddAsync sonrası reservation.Id set ediliyorsa o kullanılır.
            _logger.LogInformation("Reservation record created with Id {ReservationId} for product {ProductId}", reservation.Id, productId);


            // 4) YENİ: RabbitMQ'ya zaman aşımı mesajı gönder
            var timeoutMessage = new ReservationTimeoutMessage
            {
                ReservationId = reservation.Id, // Veritabanındaki ActionReservation'ın ID'si
                ProductId = productId,
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };

            try
            {
                _messagePublisher.PublishReservationTimeoutMessage(timeoutMessage);
                _logger.LogInformation("Reservation timeout message published for ReservationId: {ReservationId}", reservation.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish reservation timeout message for ReservationId: {ReservationId}. Reservation might not expire automatically.", reservation.Id);
                // HATA YÖNETİMİ:
                // Bu durumda ne yapmalı?
                // 1. Lock'u hemen serbest bırak? (await _lockService.ReleaseLockAsync(lockKey);)
                // 2. Rezervasyonu 'Failed' olarak işaretle?
                // 3. İşlemi tamamen geri al (rollback)?
                // Şimdilik sadece logluyoruz ama bu kritik bir nokta. En güvenlisi işlemi geri almaktır.
                // Bu durumda lock'un da serbest bırakılması gerekir.
                // Veya mesajı bir "retry" mekanizmasına sokmak.
                // Basitlik adına, şu an sadece hatayı yukarı fırlatabiliriz.
                await _lockService.ReleaseLockAsync(lockKey); // Mesaj gönderilemediyse lock'u bırakmak mantıklı olabilir
                throw; // Hatanın üst katmanlara bildirilmesi için
            }

            // Eğer mesaj başarıyla gönderildiyse, lock kalmalı (zaman aşımı servisi veya ürün alınınca bırakılacak)
            // Ancak sizin mevcut Redis lock süreniz (TimeSpan.FromMinutes(10)) zaten zaman aşımıyla aynı.
            // Bu durumda RabbitMQ zaman aşımı mesajı gelene kadar Redis lock da aktif olacaktır.
            // Eğer ürün alınırsa hem rezervasyon 'Completed' yapılır hem de Redis lock erken bırakılır.

            return reservation; // Başarılı yanıt
        }

        public async Task CompleteReservationAsync(Guid reservationId)
        {
            var reservation = await _resRepo.GetByIdAsync(reservationId)
       ?? throw new NotFoundException($"Rezervasyon '{reservationId}' bulunamadı.");

            // 2) Durum kontrolü
            if (reservation.Status != ActionStatus.Pending)
                throw new BusinessException(
                    $"Rezervasyon durumu '{reservation.Status}' olduğundan tamamlanamaz.");

            // 3) Rezervasyonu tamamla
            reservation.Status = ActionStatus.Completed;
            await _resRepo.UpdateAsync(reservation);

            // 4) Redis kilidini kaldır
            await _lockService.ReleaseLockAsync($"product:lock:{reservation.ProductId}");

            // 5) Ürünü pasif hale getir
            var product = await _productRepo.GetByIdAsync(reservation.ProductId)
                ?? throw new NotFoundException($"Ürün '{reservation.ProductId}' bulunamadı.");

            product.IsActive = false;
            await _productRepo.UpdateAsync(product);
        }

        public Task Task<ActionReservation>(Guid userId, Guid productId)
        {
            throw new NotImplementedException();
        }
    }
}