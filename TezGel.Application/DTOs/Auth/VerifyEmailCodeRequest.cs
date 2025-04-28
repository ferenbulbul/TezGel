using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.DTOs.Auth
{
    public class VerifyEmailCodeRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}