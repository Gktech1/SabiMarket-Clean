using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

namespace SabiMarket.Application.Validators
{
    // CreateGoodBoyValidator.cs
    public class CreateGoodBoyValidator : AbstractValidator<CreateGoodBoyDto>
    {
        public CreateGoodBoyValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^(\+234|0)[789][01]\d{8}$").WithMessage("Please enter a valid Nigerian phone number")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(x => new[] { "Male", "Female", "Other" }.Contains(x))
                .WithMessage("Gender must be either 'Male', 'Female', or 'Other'");
        }
    }
}
