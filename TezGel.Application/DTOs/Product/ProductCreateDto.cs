using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace TezGel.Application.DTOs.Product
{
    public class ProductCreateRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public Guid CategoryId { get; set; }
        public string ImagePath { get; set; }

    }

}