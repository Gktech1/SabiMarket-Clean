namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateAdminProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Gender { get; set; }
        public string ProfileImageUrl { get; set; }
    }

    public class UpdateAdminAccessDto
    {
        public bool HasDashboardAccess { get; set; }
        public bool HasRoleManagementAccess { get; set; }
        public bool HasTeamManagementAccess { get; set; }
        public bool HasAuditLogAccess { get; set; }
    }

    public class AdminFilterRequestDto
    {
        public string SearchTerm { get; set; }
        public string Department { get; set; }
        public string AdminLevel { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AdminDashboardStatsDto
    {
        public int RegisteredLGAs { get; set; }
        public int ActiveChairmen { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
