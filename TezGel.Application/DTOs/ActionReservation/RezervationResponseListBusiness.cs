using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Enums;

namespace TezGel.Application.DTOs.ActionReservation
{
    public class RezervationResponseListBusiness
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        public Guid ProductId { get; set; }
        public string ProductName { get; set; }

        public DateTime ExpireAt { get; set; }

        public double CustomerLatitude { get; set; }
        public double CustomerLongitude { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }

    }
}