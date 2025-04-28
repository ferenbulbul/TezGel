using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TezGel.Domain.Common;
using TezGel.Domain.Enums;

namespace TezGel.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public UserType UserType { get; set; }
        public CustomerUser? CustomerUser { get; set; }
        public BusinessUser? BusinessUser { get; set; }
    }
}