using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly ITokenService _tokenService;
        private readonly ICustomerUserRepository _customerUserRepository;
        private readonly IBusinessUserRepository _businessUserRepository;
        IMailService _mailService;
        private readonly IConfiguration _configuration;

        public AuthManager(
            UserManager<AppUser> userManager,
            ICustomerUserRepository customerUserRepository,
            IBusinessUserRepository businessUserRepository,
            IConfiguration configuration, ITokenService tokenService, IMailService mailService)
        {
            _userManager = userManager;
            _customerUserRepository = customerUserRepository;
            _businessUserRepository = businessUserRepository;
            _configuration = configuration;
            _tokenService = tokenService;
            _mailService = mailService;
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
                throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));
            }

            var business = new BusinessUser
            {
                Id = user.Id,
                CompanyName = dto.CompanyName,
                CompanyType = dto.CompanyType
            };
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);



            await _businessUserRepository.AddAsync(business);
        }

        public async Task<(string AccessToken, string RefreshToken, bool EmailConfirmed)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                throw new Exception("Geçersiz e mail adresi  veya şifre hatalı.");


            var (accessToken, refreshToken) = await _tokenService.CreateTokenAsync(user);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpireDate = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return (accessToken, refreshToken, user.EmailConfirmed);
        }

        public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpireDate <= DateTime.UtcNow)
            {
                throw new Exception("Invalid or expired refresh token.");
            }

            var (newAccessToken, newRefreshToken) = await _tokenService.CreateTokenAsync(user);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpireDate = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return (newAccessToken, newRefreshToken);
        }

        public async Task VerifyEmailCodeAsync(string email, string code)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("Kullanıcı bulunamadı.");

            if (user.EmailVerificationCode != code)
                throw new Exception("Kod yanlış.");

            if (user.EmailVerificationExpireTime == null || user.EmailVerificationExpireTime < DateTime.UtcNow)
                throw new Exception("Kodun süresi dolmuş.");

            user.EmailConfirmed = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationExpireTime = null;
            await _userManager.UpdateAsync(user);
        }

        public async Task CreateEmailCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("Kullanıcı bulunamadı.");



            var verificationCode = new Random().Next(100000, 999999).ToString();
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationExpireTime = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            await _mailService.SendEmailAsync(email, "Email Doğrulama Kodu", $"Doğrulama kodunuz: {verificationCode}");
        }
    }


}