// DTOs/Responses/RoleResponseDto.cs
using SabiMarket.Application.DTOs.Requests;

public class RoleResponseDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> AllPermissions { get; set; } = new();
    public List<string> VisiblePermissions
    {
        get
        {
            return AllPermissions
                .Take(RolePermissionConstants.VisiblePermissionsCount)
                .ToList();
        }
    }
    public int AdditionalPermissionsCount
    {
        get
        {
            var remaining = AllPermissions.Count - RolePermissionConstants.VisiblePermissionsCount;
            return remaining > 0 ? remaining : 0;
        }
    }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string LastModifiedBy { get; set; }

    public class RolePermissionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsGranted { get; set; }
    }

    public class AdminRoleResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public List<string> Permissions { get; set; }
    }
}
