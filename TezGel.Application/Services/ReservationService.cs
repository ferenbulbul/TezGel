using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        public ReservationService(
            ILockService lockService,
            IProductRepository productRepo,
            IActionRepository resRepo)
        {
            _lockService = lockService;
            _productRepo = productRepo;
            _resRepo = resRepo;
        }

        public async Task ReserveProductAsync(Guid userId, Guid productId)
        {
            // 1) Ürün var mı, aktif mi, tarihi geçmiş mi?
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null || !product.IsActive || product.ExpireAt < DateTime.UtcNow)
                throw new InvalidOperationException("Ürün rezerve edilemez.");

            // 2) Redis lock
            var lockKey = $"product:lock:{productId}";
            var locked = await _lockService.TryAcquireLockAsync(lockKey, TimeSpan.FromMinutes(10));
            if (!locked)
                throw new InvalidOperationException("Ürün başka bir kullanıcıya zaten rezerve edilmiş veya rezerve aşamasında  .");

            var reservation = new ActionReservation
            {
                UserId = userId,
                ProductId = productId,
                ExpireAt = DateTime.UtcNow.AddMinutes(1),
                Status = ActionStatus.Pending
            };
            await _resRepo.AddAsync(reservation);
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
    }
}