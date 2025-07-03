using FluentValidation;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.Validators
{
    // UpdateProfileDtoValidator
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {

        public UpdateProfileDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .Matches("^[a-zA-Z ]*$").WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.EmailAddress)
                .NotEmpty().WithMessage("Email address is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email address cannot exceed 100 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^[0-9]{11}$").WithMessage("Phone number must be exactly 11 digits")
                .Must(phone => phone.StartsWith("0")).WithMessage("Phone number must start with 0");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.LocalGovernmentId)
                .NotEmpty().WithMessage("Local Government ID is required")
                .MaximumLength(50).WithMessage("Local Government ID cannot exceed 50 characters");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(500).WithMessage("Profile image URL cannot exceed 500 characters")
                .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ProfileImageUrl))
                .WithMessage("Invalid URL format for profile image");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    } 
}
