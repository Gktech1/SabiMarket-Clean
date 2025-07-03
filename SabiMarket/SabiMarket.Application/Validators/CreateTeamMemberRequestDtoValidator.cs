using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.Validators
{
    using FluentValidation;
    using SabiMarket.Application.DTOs.Requests;

    public class CreateTeamMemberRequestDtoValidator : AbstractValidator<CreateTeamMemberRequestDto>
    {
        public CreateTeamMemberRequestDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]*$").WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\d{11}$").WithMessage("Phone number must be exactly 11 digits");

            RuleFor(x => x.EmailAddress)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Invalid email address format")
                .MaximumLength(256).WithMessage("Email address cannot exceed 256 characters");
        }
    }

 /*   public class UpdateTeamMemberRequestDtoValidator : AbstractValidator<UpdateTeamMemberRequestDto>
    {
        public UpdateTeamMemberRequestDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]*$").WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\d{11}$").WithMessage("Phone number must be exactly 11 digits");

            RuleFor(x => x.EmailAddress)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Invalid email address format")
                .MaximumLength(256).WithMessage("Email address cannot exceed 256 characters");
        }
    }*/
}
