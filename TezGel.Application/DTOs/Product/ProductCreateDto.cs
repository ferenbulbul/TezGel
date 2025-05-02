using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TezGel.Application.DTOs.Product
{
     public class ProductCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public Guid CategoryId { get; set; }
        public DateTime ExpireAt { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IFormFile ImageFile { get; set; }
        public Guid BusinessUserId { get; set; }
    }
    
}