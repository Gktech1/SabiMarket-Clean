using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

public class CreateChairmanRequestDtoValidator : AbstractValidator<CreateChairmanRequestDto>
{
    public CreateChairmanRequestDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters")
            .Matches("^[a-zA-Z ]*$").WithMessage("Full name can only contain letters and spaces");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^[0-9]{11}$").WithMessage("Phone number must be exactly 11 digits")
            .Must(phone => phone.StartsWith("0")).WithMessage("Phone number must start with 0");
       
        /*RuleFor(x => x.MarketId)
            .NotEmpty().WithMessage("Market ID is required")
            .MaximumLength(50).WithMessage("Market ID cannot exceed 50 characters");*/

        /*  RuleFor(x => x.Password)
              .NotEmpty().WithMessage("Password is required")
              .MinimumLength(8).WithMessage("Password must be at least 8 characters")
              .MaximumLength(50).WithMessage("Password cannot exceed 50 characters")
              .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
              .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
              .Matches("[0-9]").WithMessage("Password must contain at least one number")
              .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");*/
    }
}