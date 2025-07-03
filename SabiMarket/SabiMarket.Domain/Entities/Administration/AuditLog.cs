using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SabiMarket.Domain.Entities
{
    [Table("AuditLogs")]
    public class AuditLog : BaseEntity
    {
        public string? UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(20)]  // For format like "9:00 am"
        public string Time { get; set; }

        [Required]
        [MaxLength(255)]
        public string Activity { get; set; }

        // Additional metadata
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Module { get; set; }

        [MaxLength(500)]
        public string Details { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; }

        // Helper method for setting Date and Time consistently
        public void SetDateTime(DateTime dateTime)
        {
            Date = dateTime.Date;
            Time = dateTime.ToString("h:mm tt"); // Formats like "9:00 am" as shown in UI
        }
    }
}

