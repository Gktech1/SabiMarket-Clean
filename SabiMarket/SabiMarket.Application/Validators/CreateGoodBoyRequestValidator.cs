using FluentValidation;
using SabiMarket.Application.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.Validators
{
    public class CreateGoodBoyRequestValidator : AbstractValidator<CreateGoodBoyRequestDto>
    {
        public CreateGoodBoyRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters");

            RuleFor(x => x.LastName)
               .NotEmpty().WithMessage("Full name is required")
               .MinimumLength(3).WithMessage("Full name must be at least 3 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required");

            RuleFor(x => x.MarketId)
                .NotEmpty().WithMessage("Market ID is required");

            // Add other validation rules as needed
        }
    }

    public class UpdateGoodBoyProfileValidator : AbstractValidator<UpdateGoodBoyProfileDto>
    {
        public UpdateGoodBoyProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters")
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("A valid email address is required")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            // Add other validation rules as needed
        }
    }
}
