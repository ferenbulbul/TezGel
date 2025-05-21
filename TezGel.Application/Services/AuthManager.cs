using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TezGel.Application.DTOs.Auth;
using TezGel.Application.Expection;
using TezGel.Application.Expections;
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
        IRedisEmailVerificationService _redisEmailVerificationService;
        private readonly IConfiguration _configuration;

        public AuthManager(
            UserManager<AppUser> userManager,
            ICustomerUserRepository customerUserRepository,
            IBusinessUserRepository businessUserRepository,
            IConfiguration configuration,
            ITokenService tokenService,
            IMailService mailService,
            IRedisEmailVerificationService redisEmailVerificationService)
        {
            _userManager = userManager;
            _customerUserRepository = customerUserRepository;
            _businessUserRepository = businessUserRepository;
            _configuration = configuration;
            _tokenService = tokenService;
            _mailService = mailService;
            _redisEmailVerificationService = redisEmailVerificationService;
        }

        public async Task RegisterCustomerAsync(CustomerRegisterRequest dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ValidationException("Bu e-posta adresi zaten kullanımda.");

            var user = new AppUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                Email = dto.Email,
                UserType = UserType.Customer,
                Latitude=dto.Latitude,
                Longitute=dto.Longitute
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errorDescriptions = result.Errors.Select(e => e.Description).ToList();
                throw new ValidationException(string.Join(", ", errorDescriptions));
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
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ValidationException("Bu e-posta adresi zaten kullanımda.");

            var user = new AppUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,   
                UserName = dto.UserName,
                Email = dto.Email,
                UserType = UserType.Business,
                Latitude=dto.Latitude,
                Longitute=dto.Longitute
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new ValidationException(errors);
            }

            var business = new BusinessUser
            {
                Id = user.Id,
                ClosingTime = dto.ClosingTime,
                CompanyName = dto.CompanyName,
                CompanyType = dto.CompanyType
            };

            await _businessUserRepository.AddAsync(business);
        }
        public async Task<(string AccessToken, string RefreshToken, bool EmailConfirmed)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new NotFoundException("Kullanıcı bulunamadı.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
                throw new ValidationException("Şifre yanlış.");


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
                throw new NotFoundException("Kullanıcı bulunamadı.");


            var isValid = await _redisEmailVerificationService.ValidateEmailVerificationCodeAsync(user.Id.ToString(), code);
            if (isValid)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                throw new ValidationException("Geçersiz doğrulama kodu.");
            }
        }
        public async Task CreateEmailCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new NotFoundException("Kullanıcı bulunamadı.");



            var verificationCode = new Random().Next(100000, 999999).ToString();

            await _redisEmailVerificationService.SetEmailVerificationCodeAsync(user.Id.ToString(), verificationCode);

            await _mailService.SendEmailAsync(email, "Email Doğrulama Kodu", $"Doğrulama kodunuz: {verificationCode}");
        }
    }


}