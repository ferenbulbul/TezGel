using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.DTOs.Product
{
    public class ProductListResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string ImagePath { get; set; }
        public DateTime ExpireAt { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CategoryName { get; set; }
    }
}