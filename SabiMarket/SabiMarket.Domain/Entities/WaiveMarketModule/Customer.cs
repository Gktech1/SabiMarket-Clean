using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    [Table("Customers")]
    public class Customer : BaseEntity
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string LocalGovernmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public DateTime SubscriptionEndDate { get; set; }
        public bool IsSubscriptionActive { get; set; }

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; }

        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual WaivedProduct WaivedProduct { get; set; }

        [ForeignKey("LocalGovernmentId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual LocalGovernment LocalGovernment { get; set; }

        public virtual ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
        public virtual ICollection<CustomerFeedback> Feedbacks { get; set; } = new List<CustomerFeedback>();
        public virtual ICollection<WaiveMarketNotification> WaiveMarketNotifications { get; set; } = new List<WaiveMarketNotification>();

    }
}
