using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.MarketParticipants
{
    [Table("AssistCenterOfficers")]
    public class AssistCenterOfficer : BaseEntity
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string ChairmanId { get; set; }

        public string? MarketId { get; set; }
        public string? UserLevel { get; set; }

        [Required]
        public string LocalGovernmentId { get; set; }

        public bool IsBlocked { get; set; } = false;

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("MarketId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Market Market { get; set; }

        [ForeignKey("ChairmanId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Chairman Chairman { get; set; }

        [ForeignKey("LocalGovernmentId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual LocalGovernment LocalGovernment { get; set; }

        // Add to AssistCenterOfficer class
        public virtual ICollection<OfficerMarketAssignment> MarketAssignments { get; set; } = new List<OfficerMarketAssignment>();

    }
}