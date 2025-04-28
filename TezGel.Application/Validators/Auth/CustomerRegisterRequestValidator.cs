using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using TezGel.Application.DTOs.Auth;

namespace TezGel.Application.Validators.Auth
{
    public class CustomerRegisterRequestValidator : AbstractValidator<CustomerRegisterRequest>
    {
        public CustomerRegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçersiz e-posta adresi");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.Address).NotEmpty();
            RuleFor(x => x.BirthDate).LessThan(DateTime.Now).WithMessage("Doğum tarihi geçmiş bir tarih olmalıdır.");
        }
    }
}