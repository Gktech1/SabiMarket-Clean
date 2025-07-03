using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabiMarket.Domain.Entities.LevyManagement
{
    [Table("LevySetup")]
    public class LevySetup : BaseEntity
    {
        public string? ChairmanId { get; set; }
        public string? UserId { get; set; }   
        public string? MarketId { get; set; }
        public PaymentPeriodEnum PaymentFrequency {  get; set; }
        public MarketTypeEnum OccupancyType { get; set; }
        public decimal Amount { get; set; }
        public bool IsSetupRecord { get; set; }

        [ForeignKey("ChairmanId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Chairman Chairman { get; set; }

        public virtual Market Market { get; set; }

        public virtual ApplicationUser User { get; set; }


    }
}
