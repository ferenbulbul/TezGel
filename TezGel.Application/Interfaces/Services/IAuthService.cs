using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.DTOs.Auth;

namespace TezGel.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task RegisterCustomerAsync(CustomerRegisterRequest dto);
        Task RegisterBusinessAsync(BusinessRegisterRequest dto);
        Task<string> LoginAsync(string email, string password);

    }

}