using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.Expections;
using TezGel.Application.Interfaces.Services;

namespace TezGel.Infrastructure.Services
{
    public class RedisEmailVerificationService : IRedisEmailVerificationService
    {
        private readonly IRedisService _redisService;
        private const string EmailVerificationPrefix = "email-verification:";

        public RedisEmailVerificationService(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task SetEmailVerificationCodeAsync(string userId, string code, TimeSpan? expiry = null)
        {
            expiry ??= TimeSpan.FromMinutes(5); 
            var key = $"{EmailVerificationPrefix}{userId}";
            await _redisService.SetAsync(key, code, expiry);
        }

        public async Task<bool> ValidateEmailVerificationCodeAsync(string userId, string code)
        {
            var key = $"{EmailVerificationPrefix}{userId}";
            var storedCode = await _redisService.GetAsync(key);

            return storedCode == code; // Kod doğru mu kontrol
        }
    }
}