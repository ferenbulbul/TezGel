using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TezGel.Application.DTOs.Auth;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Application.Interfaces.Services;
using TezGel.Domain.Entities;
using TezGel.Domain.Enums;

namespace TezGel.Application.Services
{

    public class AuthManager : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICustomerUserRepository _customerUserRepository;
        private readonly IBusinessUserRepository _businessUserRepository;
        private readonly IConfiguration _configuration;

        public AuthManager(
            UserManager<AppUser> userManager,
            ICustomerUserRepository customerUserRepository,
            IBusinessUserRepository businessUserRepository,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _customerUserRepository = customerUserRepository;
            _businessUserRepository = businessUserRepository;
            _configuration = configuration;
        }

        public async Task RegisterCustomerAsync(CustomerRegisterRequest dto)
        {
            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                UserType = UserType.Customer
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
            }

            var customer = new CustomerUser
            {
                Id = user.Id,
                Address = dto.Address,
                BirthDate = dto.BirthDate
            };

            await _customerUserRepository.AddAsync(customer);
        }

        public async Task RegisterBusinessAsync(BusinessRegisterRequest dto)
        {
            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                UserType = UserType.Business
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errorMessages = new List<string>();

                foreach (var error in result.Errors)
                {
                    switch (error.Code)
                    {
                        case "DuplicateEmail":
                            errorMessages.Add("Bu email adresi zaten kayıtlı.");
                            break;
                        case "DuplicateUserName":
                            errorMessages.Add("Bu kullanıcı adı zaten kayıtlı.");
                            break;
                        case "PasswordTooShort":
                            errorMessages.Add("Şifre çok kısa, en az 6 karakter olmalı.");
                            break;
                        default:
                            errorMessages.Add("Beklenmeyen bir hata oluştu.");
                            break;
                    }
                }

                throw new BusinessException(string.Join(" | ", errorMessages));
            }

            var business = new BusinessUser
            {
                Id = user.Id,
                CompanyName = dto.CompanyName,
                TaxNumber = dto.TaxNumber
            };

            await _businessUserRepository.AddAsync(business);
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            // Şu an token işleri yapmıyoruz, placeholder:
            throw new NotImplementedException();
        }
    }

}