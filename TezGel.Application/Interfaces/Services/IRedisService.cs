using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.Interfaces.Services
{
    public interface IRedisService
    {
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetAsync(string key);
        Task<bool> DeleteAsync(string key);
    }
}