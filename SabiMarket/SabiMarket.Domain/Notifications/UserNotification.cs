using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("UserNotifications")]
public class UserNotification : BaseEntity
{
    [Required]
    [StringLength(450)] // ✅ Changed from 50 to 450 to match ASP.NET Identity
    public string UserId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    [StringLength(1000)]
    public string Body { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(2000)]
    public string? DataJson { get; set; } // Store as JSON string

    public bool IsRead { get; set; } = false;

    public NotificationType Type { get; set; } = NotificationType.General;

    [StringLength(450)] // ✅ Also updated this for consistency
    public string? RelatedEntityId { get; set; } // For orders, products, etc.

    [StringLength(500)]
    public string? ActionUrl { get; set; } // Deep link URL

    // Navigation properties
    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual ApplicationUser User { get; set; }
}