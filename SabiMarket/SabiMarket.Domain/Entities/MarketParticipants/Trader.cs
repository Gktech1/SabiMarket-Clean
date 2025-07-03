using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.MarketParticipants
{
    [Table("Traders")]
    public class Trader : BaseEntity
    {
        [Required]  
        public string UserId { get; set; }
        public string MarketId { get; set; }
        public string? ChairmanId { get; set; }  
        public string? SectionId { get; set; }
        public string CaretakerId { get; set; }

        [Required]
        [StringLength(50)]
        public string? TIN { get; set; }

        [Required]
        public string? BusinessName { get; set; }

        public string? TraderName { get; set; }

        public string? MarketName { get; set; }

        public string BusinessType { get; set; }

        public string? QRCode { get; set; }

        public decimal? Amount { get; set; }   = default(decimal?);    

        public PaymentPeriodEnum? PaymentFrequency { get; set; }    

        public MarketTypeEnum TraderOccupancy { get; set; }


        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("MarketId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Market Market { get; set; }

        [ForeignKey("SectionId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual MarketSection Section { get; set; }

        [ForeignKey("CaretakerId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Caretaker Caretaker { get; set; }
        public virtual Chairman Chairman { get; set; }
        // New: Collection of building types
        public virtual ICollection<TraderBuildingType> BuildingTypes { get; set; } = new List<TraderBuildingType>();

        public virtual ICollection<LevyPayment> LevyPayments { get; set; } = new List<LevyPayment>();
    }
}