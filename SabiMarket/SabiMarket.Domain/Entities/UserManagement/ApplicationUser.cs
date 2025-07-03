
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabiMarket.Domain.Entities.UserManagement
{
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser<string>
    {
        [Required]
        [PersonalData]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [PersonalData]
        [StringLength(100)]
        public string LastName { get; set; }

        public string? Address { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsBlocked { get; set; } = false;
        public string? Gender { get; set; }
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public string? PasswordResetToken { get; set; }  // Custom column for password reset
        public DateTime? PasswordResetExpiry { get; set; } // Expiry timestamp
        public string? RefreshToken { get; set; }
        public bool? PasswordResetVerified { get; set; } = false;
        public DeliveryMethod? PasswordResetMethod { get; set; }
        public string? RefreshTokenJwtId { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool? IsRefreshTokenUsed { get; set; }
        public string? LocalGovernmentId { get; set; }
        public string? AdminId { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Admin Admin { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Chairman Chairman { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Trader Trader { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Vendor Vendor { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Customer Customer { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Caretaker Caretaker { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual GoodBoy GoodBoy { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual AssistCenterOfficer AssistCenterOfficer { get; set; }

        [ForeignKey("LocalGovernmentId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual LocalGovernment LocalGovernment { get; set; }
    }
}