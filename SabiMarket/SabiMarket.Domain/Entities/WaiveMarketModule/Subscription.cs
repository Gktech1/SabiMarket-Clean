using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    [Table("Subscriptions")]
    public class Subscription : BaseEntity
    {

        [Required]
        public string SubscriberId { get; set; }

        public string? SubscriptionActivatorId { get; set; }
        public string SubscriberType { get; set; }

        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string ProofOfPayment { get; set; }
        public bool IsSubscriberConfirmPayment { get; set; }
        public bool IsAdminConfirmPayment { get; set; }

        [ForeignKey("SubscriberId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]  // <-- Add this
        public virtual ApplicationUser Subscriber { get; set; }

        [ForeignKey("SubscriptionActivatorId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]  // <-- Add this
        public virtual ApplicationUser SubscriptionActivator { get; set; }
    }
}