using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Domain.Entities;

namespace TezGel.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<(string AccessToken, string RefreshToken)> CreateTokenAsync(AppUser user);
    }
}