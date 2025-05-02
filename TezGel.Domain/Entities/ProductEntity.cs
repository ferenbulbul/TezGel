using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Common;

namespace TezGel.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string ImagePath { get; set; }
        public DateTime ExpireAt { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // İlişkiler
        public Guid BusinessUserId { get; set; }
        public BusinessUser BusinessUser { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        public bool IsActive { get; set; } = true;
    }

}