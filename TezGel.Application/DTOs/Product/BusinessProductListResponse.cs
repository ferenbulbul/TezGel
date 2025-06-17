using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.DTOs.Product
{
    public class BusinessProductListResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string ImagePath { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
        public bool? IsReserved;
    }
}