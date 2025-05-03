using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using TezGel.Application.Interfaces.Services;

namespace TezGel.Infrastructure.Services
{
    public class RedisLockService : ILockService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisLockService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task<bool> TryAcquireLockAsync(string key, TimeSpan expiration)
        {
            // NX: yoksa set et, expiry ile süreli kilit
            return await _db.StringSetAsync(
                key,
                "locked",
                expiration,
                when: When.NotExists
            );
        }

        public async Task ReleaseLockAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }
         public async Task<bool> IsLockedAsync(string key) 
         => await _db.KeyExistsAsync(key);
    }



}