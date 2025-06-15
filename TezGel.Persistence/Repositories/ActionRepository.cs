using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TezGel.Application.DTOs.ActionReservation;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Domain.Entities;
using TezGel.Persistence.Context;

namespace TezGel.Persistence.Repositories
{
    public class ActionRepository : GenericRepository<ActionReservation>, IActionRepository
    {
        public ActionRepository(TezGelDbContext context) : base(context)
        {
        }
        public async Task<List<RezervationResponseList>> GetReservationsAsync(Guid userId)
        {
            var reservations = await _context.ActionReservations
                .Include(r => r.Product)
                .ThenInclude(r => r.BusinessUser)
            .Where(r=>r.UserId ==userId)
                .ToListAsync();

            return reservations.Select(r => new RezervationResponseList
            {
                Id = r.Id,
                UserId = r.UserId,
                CompanyName = r.Product.BusinessUser.CompanyName,
                Latitude = r.Product.Latitude,
                Longitude = r.Product.Longitude,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                CreatedDate = r.CreatedDate,
                ExpireAt = r.ExpireAt,
                Status = r.Status.ToString(),
                ClosingTime = r.Product.BusinessUser.ClosingTime,
                ImagePath = r.Product.ImagePath
            }).ToList();
        }
    }
}