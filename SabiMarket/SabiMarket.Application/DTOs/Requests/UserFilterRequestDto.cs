// DTOs for GetAllUsers endpoint
using System.ComponentModel.DataAnnotations;

public class UserFilterRequestDto
{
    public string? SearchTerm { get; set; }
    public string? UserType { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public string? RoleName { get; set; }
}

public class UserResponseDto
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Updated to include both role names and IDs
    public List<string> Roles { get; set; } = new List<string>();
    public List<UserRoleDto> RolesWithIds { get; set; } = new List<UserRoleDto>();

    public string UserType { get; set; }

    // Additional properties that may be populated based on user type
    public string Department { get; set; }
    public string Position { get; set; }
    public string LocalGovernmentName { get; set; }
    public string MarketName { get; set; }
}

public class UserRoleDto
{
    public string Id { get; set; }
    public string Name { get; set; }
}
// DTOs for AssignUserRoleAndPermissions endpoint
public class AssignRoleRequestDto
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public string RoleId { get; set; }

    public bool RemoveExistingRoles { get; set; } = false;

    public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();

    // Admin-specific properties if assigning admin role
    public string AdminLevel { get; set; }
    public string Department { get; set; }
    public string Position { get; set; }
    public bool? HasDashboardAccess { get; set; }
    public bool? HasRoleManagementAccess { get; set; }
    public bool? HasTeamManagementAccess { get; set; }
    public bool? HasAuditLogAccess { get; set; }
    public bool? HasAdvertManagementAccess { get; set; }
}

public class PermissionDto
{
    public string Name { get; set; }
    public bool IsGranted { get; set; }
}

public class AssignRoleResponseDto
{
    public string UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserFullName { get; set; }
    public string AssignedRole { get; set; }
    public List<string> PreviousRoles { get; set; } = new List<string>();
    public List<string> CurrentRoles { get; set; } = new List<string>();
    public List<PermissionDto> AssignedPermissions { get; set; } = new List<PermissionDto>();
}
