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
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public UserType UserType { get; set; }
        public string? RefreshToken { get; set; }
        public double Longitute { get; set; }
        public double Latitude { get; set; }
        public DateTime? RefreshTokenExpireDate { get; set; }
        public CustomerUser? CustomerUser { get; set; }
        public BusinessUser? BusinessUser { get; set; }


    }
}