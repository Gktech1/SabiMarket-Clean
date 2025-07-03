
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Domain.Notifications
{
    [Table("DeviceTokens")]
    public class DeviceToken : BaseEntity
    {
        [Required]
        [StringLength(450)] // ✅ Changed from 50 to 450 to match ASP.NET Identity
        public string UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; }

        [Required]
        [StringLength(20)]
        public string? DeviceType { get; set; } // iOS, Android, Web

        [StringLength(200)]
        public string? DeviceInfo { get; set; } // Browser, OS version, etc.

        public DateTime LastUsed { get; set; } = DateTime.UtcNow.AddHours(1);

        // Navigation properties
        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual ApplicationUser User { get; set; }
    }
}


