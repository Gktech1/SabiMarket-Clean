using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.Administration
{
    public class Chairman : BaseEntity
    {
        [Required]  
        public string UserId { get; set; }

        public string? MarketId { get; set; }
        public string LocalGovernmentId { get; set; }
        public string? FullName { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Office { get; set; }

        public int TotalRecords { get; set; }
        public DateTime TermStart { get; set; }
        public DateTime? TermEnd { get; set; }
        public DateTime? LastLoginAt { get; set; }

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; }
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Market Market { get; set; }
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual LocalGovernment LocalGovernment { get; set; }
    }

}