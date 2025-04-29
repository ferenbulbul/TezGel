using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.Interfaces.Services
{
    public interface IRedisEmailVerificationService
    {
        Task SetEmailVerificationCodeAsync(string userId, string code, TimeSpan? expiry = null);
        Task<bool> ValidateEmailVerificationCodeAsync(string userId, string code);
    }
}