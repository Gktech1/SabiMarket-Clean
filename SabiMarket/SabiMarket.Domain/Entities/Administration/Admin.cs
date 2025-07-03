using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabiMarket.Domain.Entities.Administration
{
    [Table("Admins")]
    public class Admin : BaseEntity
    {
        [Required]
        public string UserId { get; set; }

        // Dashboard Stats Tracking
        public int RegisteredLGAs { get; set; }
        public int ActiveChairmen { get; set; }
        public decimal TotalRevenue { get; set; }

        // Advertisement Management
        public bool HasAdvertManagementAccess { get; set; } = true;

        // Admin Portal Specific Data
        [MaxLength(100)]
        public string AdminLevel { get; set; }
        [MaxLength(100)]
        public string Department { get; set; }
        [MaxLength(100)]
        public string Position { get; set; }

        // Access Controls
        public bool HasDashboardAccess { get; set; } = true;
        public bool HasRoleManagementAccess { get; set; } = true;
        public bool HasTeamManagementAccess { get; set; } = true;
        public bool HasAuditLogAccess { get; set; } = true;

        // Last Dashboard Access
        public DateTime? LastDashboardAccess { get; set; }

        // Stats Last Updated
        public DateTime StatsLastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
        public virtual ICollection<AuditLog> AdminAuditLogs { get; set; } = new List<AuditLog>();

    }

    /*  [Table("Admins")]
      public class Admin : BaseEntity
      {
          [Required]
          public string UserId { get; set; }

          // Dashboard Stats Tracking
          public int RegisteredLGAs { get; set; }
          public int ActiveChairmen { get; set; }
          public decimal TotalRevenue { get; set; }

          // Admin Portal Specific Data
          [MaxLength(100)]
          public string AdminLevel { get; set; }  // For different levels of admin access

          [MaxLength(100)]
          public string Department { get; set; }

          [MaxLength(100)]
          public string Position { get; set; }

          public bool HasDashboardAccess { get; set; } = true;
          public bool HasRoleManagementAccess { get; set; } = true;
          public bool HasTeamManagementAccess { get; set; } = true;
          public bool HasAuditLogAccess { get; set; } = true;

          // Last Dashboard Access
          public DateTime? LastDashboardAccess { get; set; }

          // Navigation Properties
          public virtual ApplicationUser User { get; set; }
          public virtual ICollection<AuditLog> AdminAuditLogs { get; set; }

          // Stats Last Updated
          public DateTime StatsLastUpdatedAt { get; set; } = DateTime.UtcNow;
      }*/
}
