using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

namespace SabiMarket.Application.Validators
{
    public class CreateAdminRequestValidator : AbstractValidator<CreateAdminRequestDto>
    {
        public CreateAdminRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Please enter a valid phone number");

            RuleFor(x => x.Position)
                .NotEmpty().WithMessage("Position is required")
                .MaximumLength(100).WithMessage("Position cannot exceed 100 characters");

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required")
                .MaximumLength(100).WithMessage("Department cannot exceed 100 characters");

            RuleFor(x => x.AdminLevel)
                .NotEmpty().WithMessage("Admin level is required")
                .MaximumLength(50).WithMessage("Admin level cannot exceed 50 characters");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(x => x.ToLower() is "male" or "female" or "other")
                .WithMessage("Gender must be either 'Male', 'Female', or 'Other'");

            RuleFor(x => x.ProfileImageUrl)
                .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Please provide a valid URL for the profile image");
        }
    }

    public class UpdateAdminProfileValidator : AbstractValidator<UpdateAdminProfileDto>
    {
        public UpdateAdminProfileValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Please enter a valid phone number");

            RuleFor(x => x.Position)
                .NotEmpty().WithMessage("Position is required")
                .MaximumLength(100).WithMessage("Position cannot exceed 100 characters");

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required")
                .MaximumLength(100).WithMessage("Department cannot exceed 100 characters");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(x => x.ToLower() is "male" or "female" or "other")
                .WithMessage("Gender must be either 'Male', 'Female', or 'Other'");

            RuleFor(x => x.ProfileImageUrl)
                .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Please provide a valid URL for the profile image");
        }
    }

    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequestDto>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9 _-]+$").WithMessage("Role name can only contain letters, numbers, spaces, hyphens and underscores");

            RuleFor(x => x.Permissions)
                .NotNull().WithMessage("Permissions list cannot be null")
                .Must(x => x.Count > 0).WithMessage("At least one permission must be selected")
                .Must(x => x.All(p => RolePermissionConstants.AllPermissions.Contains(p)))
                .WithMessage("All permissions must be valid");
        }
    }

    public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequestDto>
    {
        public UpdateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9 _-]+$").WithMessage("Role name can only contain letters, numbers, spaces, hyphens and underscores");

            RuleFor(x => x.Permissions)
                .NotNull().WithMessage("Permissions list cannot be null")
                .Must(x => x.Count > 0).WithMessage("At least one permission must be selected")
                .Must(x => x.All(p => RolePermissionConstants.AllPermissions.Contains(p)))
                .WithMessage("All permissions must be valid");
        }
    }
}