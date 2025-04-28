using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.DTOs.Auth
{
     public class BusinessRegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyType { get; set; }
    }
}