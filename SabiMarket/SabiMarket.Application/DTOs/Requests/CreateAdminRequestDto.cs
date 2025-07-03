namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateAdminRequestDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string AdminLevel { get; set; }
        public string Gender { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool HasDashboardAccess { get; set; }
        public bool HasRoleManagementAccess { get; set; }
        public bool HasTeamManagementAccess { get; set; }
        public bool HasAuditLogAccess { get; set; }
    }
}
