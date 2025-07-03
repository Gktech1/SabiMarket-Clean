namespace SabiMarket.Application.DTOs.Responses
{
    public class AdminResponseDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string AdminLevel { get; set; }
        public bool HasDashboardAccess { get; set; }
        public bool HasRoleManagementAccess { get; set; }
        public bool HasTeamManagementAccess { get; set; }
        public bool HasAuditLogAccess { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
