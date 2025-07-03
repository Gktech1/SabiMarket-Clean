namespace SabiMarket.Application.DTOs.Requests
{
    // DTOs/Constants/RolePermissionConstants.cs
    public static class RolePermissionConstants
    {
        public const string ViewTraders = "ViewTraders";
        public const string ViewLevies = "ViewLevies";
        public const string ScanTradersQRCode = "ScanTradersQRCode";
        public const string AddLevies = "AddLevies";
        public const string AddTraders = "AddTraders";

        public static readonly List<string> AllPermissions = new()
    {
        ViewTraders,
        ViewLevies,
        ScanTradersQRCode,
        AddLevies,
        AddTraders
    };

        public const int VisiblePermissionsCount = 2; // For "+2 more" feature
    }

    // DTOs/Requests/CreateRoleRequestDto.cs
    public class CreateRoleRequestDto
    {
        public string Name { get; set; }
        public string Description { get; set; } 
        public List<string> Permissions { get; set; } = new();
    }

    // DTOs/Requests/UpdateRoleRequestDto.cs
    public class UpdateRoleRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }


    // DTOs/Requests/RoleFilterRequestDto.cs
    public class RoleFilterRequestDto
    {
        public string? SearchTerm { get; set; }
    }

  
}
