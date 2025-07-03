using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

namespace SabiMarket.Application.Validators
{
    public class CreateAdminValidator : AbstractValidator<CreateAdminRequestDto>
    {
        public CreateAdminValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
                .Matches("^[a-zA-Z\\s]+$").WithMessage("First name can only contain letters and spaces");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
                .Matches("^[a-zA-Z\\s]+$").WithMessage("Last name can only contain letters and spaces");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches("^[0-9]+$").WithMessage("Phone number must contain only numbers")
                .Length(11).WithMessage("Phone number must be 11 digits");

            RuleFor(x => x.Position)
                .NotEmpty().WithMessage("Position is required")
                .MaximumLength(100).WithMessage("Position cannot exceed 100 characters");

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required")
                .MaximumLength(100).WithMessage("Department cannot exceed 100 characters");

            RuleFor(x => x.AdminLevel)
                .NotEmpty().WithMessage("Admin level is required")
                .Must(level => new[] { "SuperAdmin", "Admin", "BasicAdmin" }.Contains(level))
                .WithMessage("Admin level must be either SuperAdmin, Admin, or BasicAdmin");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(gender => new[] { "Male", "Female", "Other" }.Contains(gender))
                .WithMessage("Gender must be either Male, Female, or Other");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(500).WithMessage("Profile image URL cannot exceed 500 characters")
                .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ProfileImageUrl))
                .WithMessage("Profile image URL must be a valid URL");

            // Access permissions validation
            // Note: These are bool properties so they don't need NotEmpty validation
            RuleFor(x => x)
                .Must(x => x.HasDashboardAccess ||
                          x.HasRoleManagementAccess ||
                          x.HasTeamManagementAccess ||
                          x.HasAuditLogAccess)
                .WithMessage("At least one access permission must be granted");
        }

        private bool BeAValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}