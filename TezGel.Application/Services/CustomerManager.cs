using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TezGel.Application.DTOs.Customer;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Application.Interfaces.Services;
using TezGel.Domain.Entities;

namespace TezGel.Application.Services
{
    public class CustomerManager : ICustomerService
    {
        private readonly UserManager<AppUser> _userManager;

        public CustomerManager(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task LocationUpdateAsync(Guid customerId,LocationRequest locationRequest)
        {
            var customer = await _userManager.FindByIdAsync(customerId.ToString());
            if (customer == null)
                throw new Exception("Customer not found");
            customer.Latitude = locationRequest.latitude;
            customer.Longitute = locationRequest.longitude;

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to update customer location");

            }
        }
    }
}