using FluentValidation;
using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.Validators
{
    public class CreateTraderValidator : AbstractValidator<CreateTraderRequestDto>
    {
        public CreateTraderValidator()
        {
            RuleFor(x => x.TraderName)
                .NotEmpty().WithMessage("Trader name is required")
                .MinimumLength(3).WithMessage("Trader name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Trader name cannot exceed 100 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\d{10,15}$").WithMessage("Phone number must be between 10 and 15 digits");

            // Email is optional but must be valid if provided
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("A valid email address is required")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required")
                .MinimumLength(3).WithMessage("Business name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Business name cannot exceed 100 characters");

           /* RuleFor(x => x.BusinessType)
                .NotEmpty().WithMessage("Business type is required")
                .MaximumLength(50).WithMessage("Business type cannot exceed 50 characters");*/

            // TIN is optional but must follow pattern if provided
            RuleFor(x => x.TIN)
                .MaximumLength(20).WithMessage("TIN cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.TIN));

            RuleFor(x => x.MarketId)
                .NotEmpty().WithMessage("Market ID is required");

            RuleFor(x => x.TraderOccupancy)
                .IsInEnum().WithMessage("Invalid trader occupancy type");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required");

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

    public class UpdateTraderValidator : AbstractValidator<UpdateTraderRequestDto>
    {
        public UpdateTraderValidator()
        {
            RuleFor(x => x.TraderName)
                .MinimumLength(3).WithMessage("Trader name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Trader name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.TraderName));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\d{10,15}$").WithMessage("Phone number must be between 10 and 15 digits")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            // Email is optional but must be valid if provided
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("A valid email address is required")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.BusinessName)
                .MinimumLength(3).WithMessage("Business name must be at least 3 characters")
                .MaximumLength(100).WithMessage("Business name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.BusinessName));

           /* RuleFor(x => x.BusinessType)
                .MaximumLength(50).WithMessage("Business type cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.BusinessType));*/

            // TIN is optional but must follow pattern if provided
            RuleFor(x => x.TIN)
                .MaximumLength(20).WithMessage("TIN cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.TIN));

            RuleFor(x => x.MarketId)
                .NotEmpty().WithMessage("Market ID cannot be empty")
                .When(x => !string.IsNullOrEmpty(x.MarketId));

            RuleFor(x => x.CaretakerId)
                .NotEmpty().WithMessage("Caretaker ID cannot be empty")
                .When(x => !string.IsNullOrEmpty(x.CaretakerId));

            RuleFor(x => x.TraderOccupancy)
                .IsInEnum().WithMessage("Invalid trader occupancy type")
                .When(x => x.TraderOccupancy.HasValue);

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender cannot be empty")
                .When(x => !string.IsNullOrEmpty(x.Gender));

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