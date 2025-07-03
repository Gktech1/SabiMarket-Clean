using FluentValidation;
using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs;

namespace SabiMarket.Application.Validators
{
    public class CreateAssistantOfficerValidator : AbstractValidator<CreateAssistantOfficerRequestDto>
    {
        public CreateAssistantOfficerValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\d{10,15}$").WithMessage("Phone number must be between 10 and 15 digits");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required");

            // MarketIds can be empty, but if provided each id should not be empty
            RuleForEach(x => x.MarketIds)
                .NotEmpty().WithMessage("Market ID cannot be empty")
                .When(x => x.MarketIds != null && x.MarketIds.Count > 0);

            // ProfileImage validation - if provided, check file size and type
           /* RuleFor(x => x.ProfileImage)
                .Must(BeValidImage).WithMessage("Profile image must be JPG, JPEG, or PNG and less than 2MB")
                .When(x => x.ProfileImage != null);*/
        }

        private bool BeValidImage(IFormFile file)
        {
            if (file == null)
                return true;

            // Check file size (2MB max)
            if (file.Length > 2 * 1024 * 1024)
                return false;

            // Check file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            return allowedTypes.Contains(file.ContentType.ToLower());
        }
    }

    public class UpdateAssistantOfficerValidator : AbstractValidator<UpdateAssistantOfficerRequestDto>
    {
        public UpdateAssistantOfficerValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(3).WithMessage("Full name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\d{10,15}$").WithMessage("Phone number must be between 10 and 15 digits");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required");

            // MarketIds can be empty, but if provided each id should not be empty
            RuleForEach(x => x.MarketIds)
                .NotEmpty().WithMessage("Market ID cannot be empty")
                .When(x => x.MarketIds != null && x.MarketIds.Count > 0);

            // ProfileImage validation - if provided, check file size and type
            /*RuleFor(x => x.ProfileImage)
                .Must(BeValidImage).WithMessage("Profile image must be JPG, JPEG, or PNG and less than 2MB")
                .When(x => x.ProfileImage != null);*/
        }

        private bool BeValidImage(IFormFile file)
        {
            if (file == null)
                return true;

            // Check file size (2MB max)
            if (file.Length > 2 * 1024 * 1024)
                return false;

            // Check file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            return allowedTypes.Contains(file.ContentType.ToLower());
        }
    }
}