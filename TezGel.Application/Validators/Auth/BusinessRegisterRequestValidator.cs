using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using TezGel.Application.DTOs.Auth;

namespace TezGel.Application.Validators.Auth
{
    public class BusinessRegisterRequestValidator : AbstractValidator<BusinessRegisterRequest>
    {
        public BusinessRegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçersiz e-posta adresi"); ;
            RuleFor(x => x.Password)
             .NotEmpty()
             .MinimumLength(6)
             .Matches(@"^(?=.*[A-Za-z])(?=.*\d).+$")
             .WithMessage("Password must contain at least one letter and one number.");
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.CompanyName).NotEmpty();
            RuleFor(x => x.CompanyType).NotEmpty();

        }
    }
}