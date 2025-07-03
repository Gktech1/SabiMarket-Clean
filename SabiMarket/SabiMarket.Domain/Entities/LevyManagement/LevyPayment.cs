using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.LevyManagement
{
    [Table("LevyPayments")]
    public class LevyPayment : BaseEntity
    {
        public string? ChairmanId { get; set; }
        public string? MarketId { get; set; }
        public string? TraderId { get; set; }
        public string? GoodBoyId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public MarketTypeEnum OccupancyType { get; set; }

        [Required]
        public PaymentPeriodEnum Period { get; set; }

        [Required]
        public PaymenPeriodEnum PaymentMethod { get; set; }

        [Required]
        public PaymentStatusEnum PaymentStatus { get; set; }

        public string TransactionReference { get; set; }
        public bool HasIncentive { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IncentiveAmount { get; set; }

        public DateTime PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CollectionDate { get; set; }
        public string Notes { get; set; }
        public string QRCodeScanned { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSetupRecord { get; set; } = false;

        /*[ForeignKey("TraderId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]*/
        public virtual Trader Trader { get; set; }

        /*[ForeignKey("MarketId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]*/
        public virtual Market Market { get; set; }

        /*[ForeignKey("GoodBoyId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]*/
        public virtual GoodBoy GoodBoy { get; set; }

        [ForeignKey("ChairmanId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Chairman Chairman { get; set; }
    }
}