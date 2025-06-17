using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TezGel.Application.DTOs.ActionReservation;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Domain.Entities;
using TezGel.Domain.Enums;
using TezGel.Persistence.Context;

namespace TezGel.Persistence.Repositories
{
    public class ActionRepository : GenericRepository<ActionReservation>, IActionRepository
    {
        public ActionRepository(TezGelDbContext context) : base(context)
        {
        }
        public async Task<List<RezervationResponseList>> GetReservationsByCustomerIdAsync(Guid userId)
        {
            return await _context.ActionReservations
                .Where(r => r.UserId == userId)
                .Select(r => new RezervationResponseList
                {
                    Id = r.Id,
                    CompanyName = r.Product.BusinessUser.CompanyName,
                    Latitude = r.Product.Latitude,
                    Longitude = r.Product.Longitude,
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name,
                    CreatedDate = r.CreatedDate,
                    ExpireAt = r.ExpireAt,
                    Status = r.Status.ToString(),
                    ClosingTime = r.Product.BusinessUser.ClosingTime,
                    ImagePath = r.Product.ImagePath,
                    OriginalPrice = r.Product.OriginalPrice,
                    DiscountedPrice = r.Product.DiscountedPrice
                })
                .ToListAsync();
        }
        public async Task<RezervationResponseList> GetReservationByCustomerIdAsync(Guid reservationId)
        {
            var reservation = await _context.ActionReservations
             .Where(r => r.Id == reservationId)
             .Select(r => new RezervationResponseList
             {
                 Id = r.Id,
                 CompanyName = r.Product.BusinessUser.CompanyName,
                 Latitude = r.Product.Latitude,
                 Longitude = r.Product.Longitude,
                 ProductId = r.ProductId,
                 ProductName = r.Product.Name,
                 CreatedDate = r.CreatedDate,
                 ExpireAt = r.ExpireAt,
                 Status = r.Status.ToString(),
                 ClosingTime = r.Product.BusinessUser.ClosingTime,
                 ImagePath = r.Product.ImagePath,
                 OriginalPrice = r.Product.OriginalPrice,
                 DiscountedPrice = r.Product.DiscountedPrice
             })
            .FirstOrDefaultAsync();



            return reservation ?? throw new KeyNotFoundException("Rezervasyon bulunamadı.");
        }
        public async Task<string> GetReservationStatusAsync(Guid reservationId)
        {
            var reservation = await _context.ActionReservations
                .Where(r => r.Id == reservationId && r.Status == ActionStatus.Pending)
                .FirstOrDefaultAsync();
            if (reservation == null)
            {
                throw new KeyNotFoundException("Rezervasyon bulunamadı veya durum beklemede değil.");
            }

            return reservation.Status.ToString();
        }
        public async Task<List<RezervationResponseListBusiness>> GetReservationsByBusinessIdAsync(Guid businessId)
        {
            return await _context.ActionReservations
                .Where(r => r.Product.BusinessUserId == businessId)
                .Select(r => new RezervationResponseListBusiness
                {
                    Id = r.Id,
                    FirstName = r.CustomerUser.AppUser.FirstName,
                    LastName = r.CustomerUser.AppUser.LastName,
                    CustomerLatitude = r.CustomerUser.AppUser.Latitude,
                    CustomerLongitude = r.CustomerUser.AppUser.Longitute,
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name,
                    ExpireAt = r.ExpireAt,
                    Status = r.Status.ToString(),
                    ImagePath = r.Product.ImagePath,
                    OriginalPrice = r.Product.OriginalPrice,
                    DiscountedPrice = r.Product.DiscountedPrice
                })
                .ToListAsync();
        }
        public async Task<RezervationResponseListBusiness?> GetReservationByBusinessIdAsync(Guid reservationId)
        {
            return await _context.ActionReservations
                .Where(r => r.Id == reservationId)
                .Select(r => new RezervationResponseListBusiness
                {
                    Id = r.Id,
                    FirstName = r.CustomerUser.AppUser.FirstName,
                    LastName = r.CustomerUser.AppUser.LastName,
                    CustomerLatitude = r.CustomerUser.AppUser.Latitude,
                    CustomerLongitude = r.CustomerUser.AppUser.Longitute,
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name,
                    ExpireAt = r.ExpireAt,
                    Status = r.Status.ToString(),
                    ImagePath = r.Product.ImagePath,
                    OriginalPrice = r.Product.OriginalPrice,
                    DiscountedPrice = r.Product.DiscountedPrice
                }).FirstOrDefaultAsync();

        }

    }
}