using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TezGel.Domain.Enums;
using TezGel.Persistence.Context;

namespace TezGel.Infrastructure.HostedService
{
    public class ExpiredReservationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(10);

        public ExpiredReservationBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TezGelDbContext>();

                // var toExpireActions = await db.ActionReservations
                //     .Where(r => r.Status == ActionStatus.Pending
                //              && r.ExpireAt <= DateTime.UtcNow)
                //     .ToListAsync(stoppingToken);
                var toExpireActions = new List<Domain.Entities.ActionReservation>();

                var toExpireProducts = await db.Products
                    .Where(p => p.IsActive && p.ExpireAt <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                if (toExpireActions.Any() || toExpireProducts.Any())
                {
                    toExpireActions.ForEach(r => r.Status = ActionStatus.Expired);
                    toExpireProducts.ForEach(p => p.IsActive = false);

                     _ = await db.SaveChangesAsync(stoppingToken);

                    Console.WriteLine($"[{DateTime.UtcNow}] " +
                        $"{toExpireActions.Count} rezervasyon, " +
                        $"{toExpireProducts.Count} ürün güncellendi.");
                }

                 await Task.Delay(_delay, stoppingToken);
            }
        }
    }
}