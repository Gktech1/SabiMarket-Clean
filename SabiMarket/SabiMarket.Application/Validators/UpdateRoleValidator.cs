using FluentValidation;
using SabiMarket.Application.DTOs.Requests;

public class UpdateRoleValidator : AbstractValidator<UpdateRoleRequestDto>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9\\s-_]+$").WithMessage("Role name can only contain letters, numbers, spaces, hyphens and underscores");

        RuleFor(x => x.Permissions)
            .NotNull().WithMessage("Permissions cannot be null")
            .NotEmpty().WithMessage("At least one permission is required")
            .Must(permissions => permissions != null && permissions.All(p => RolePermissionConstants.AllPermissions.Contains(p)))
            .WithMessage($"Permissions must be from the allowed list: {string.Join(", ", RolePermissionConstants.AllPermissions)}");
    }
}