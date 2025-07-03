using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateTeamMemberRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        public string? AdminLevel { get; set; }

        public string? Department { get; set; }
        public string? Position { get; set; }

        // Optional access permissions
        public bool? HasDashboardAccess { get; set; }
        public bool? HasRoleManagementAccess { get; set; }
        public bool? HasTeamManagementAccess { get; set; }
        public bool? HasAuditLogAccess { get; set; }
        public bool? HasAdvertManagementAccess { get; set; }
    }

    // UpdateTeamMemberRequestDto.cs
    public class UpdateTeamMemberRequestDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }

        public string? EmailAddress { get; set; }
    }

    // TeamMemberFilterRequestDto.cs
    public class TeamMemberFilterRequestDto
    {
        public string? SearchTerm { get; set; }
    }
}
