using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.DTOs.Product
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
    }
}