using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Common;

namespace TezGel.Domain.Entities
{
    public class TestEntity:BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        
        //public virtual ICollection<ProductEntity> Products { get; set; } = new HashSet<ProductEntity>();
        
        //public virtual ICollection<UserEntity> Users { get; set; } = new HashSet<UserEntity>();
    }
}