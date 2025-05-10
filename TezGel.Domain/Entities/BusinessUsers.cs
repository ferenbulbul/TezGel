using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Common;

namespace TezGel.Domain.Entities
{
    public class BusinessUser : BaseEntity
    {
        public Guid Id { get; set; }  // PK ve FK (AppUser.Id)
        public string CompanyName { get; set; }
        public string CompanyType { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public AppUser AppUser { get; set; }

    }
}