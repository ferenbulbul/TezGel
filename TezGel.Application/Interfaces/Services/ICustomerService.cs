using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.DTOs.Customer;

namespace TezGel.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task LocationUpdateAsync(Guid customerId, LocationRequest locationRequest);
    }
}