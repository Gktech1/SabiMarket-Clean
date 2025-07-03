using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    [Table("Vendors")]
    public class Vendor : BaseEntity
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string LocalGovernmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string BusinessName { get; set; }

        [Required]
        [StringLength(200)]
        public string BusinessAddress { get; set; }

        [Required]
        public string VendorCode { get; set; }
        public CurrencyTypeEnum? VendorCurrencyType { get; set; }

        public string BusinessDescription { get; set; }
        public VendorTypeEnum Type { get; set; }
        public bool IsVerified { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
        public bool IsSubscriptionActive { get; set; }

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("LocalGovernmentId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual LocalGovernment LocalGovernment { get; set; }

        public virtual ICollection<WaivedProduct> Products { get; set; } = new List<WaivedProduct>();
        public virtual ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
        public virtual ICollection<CustomerFeedback> Feedbacks { get; set; } = new List<CustomerFeedback>();
        public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
        public virtual ICollection<WaiveMarketNotification> WaiveMarketNotifications { get; set; } = new List<WaiveMarketNotification>();
    }

}
