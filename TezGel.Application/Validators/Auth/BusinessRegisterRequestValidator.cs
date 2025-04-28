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
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.CompanyName).NotEmpty();
            RuleFor(x => x.CompanyType).NotEmpty();
            
        }
    }
}