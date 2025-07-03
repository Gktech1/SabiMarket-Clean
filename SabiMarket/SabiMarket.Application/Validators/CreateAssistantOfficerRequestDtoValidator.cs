using FluentValidation;
using SabiMarket.Application.DTOs;

public class CreateAssistantOfficerRequestDtoValidator : AbstractValidator<CreateAssistantOfficerRequestDto>
{
    public CreateAssistantOfficerRequestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s]*$").WithMessage("Full name can only contain letters and spaces");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^[0-9]{11}$").WithMessage("Phone number must be exactly 11 digits")
            .Must(phone => phone.StartsWith("0")).WithMessage("Phone number must start with 0");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required")
            .Must(gender => gender == "Male" || gender == "Female" || gender == "Other")
            .WithMessage("Gender must be 'Male', 'Female', or 'Other'");

        RuleForEach(x => x.MarketIds)
      .NotEmpty().WithMessage("Market ID cannot be empty")
      .MaximumLength(50).WithMessage("Market ID cannot exceed 50 characters");
    }
}