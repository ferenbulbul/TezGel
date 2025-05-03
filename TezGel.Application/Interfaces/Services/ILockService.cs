using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TezGel.Application.Interfaces.Services
{
     public interface ILockService
    {
        Task<bool> TryAcquireLockAsync(string key, TimeSpan expiry);
        Task ReleaseLockAsync(string key);
        Task<bool> IsLockedAsync(string key);
    }
}