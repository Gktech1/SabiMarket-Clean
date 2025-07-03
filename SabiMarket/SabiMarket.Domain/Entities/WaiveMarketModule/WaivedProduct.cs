using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.OrdersAndFeedback;
using SabiMarket.Domain.Entities.Supporting;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    [Table("WaivedProducts")]
    public class WaivedProduct : BaseEntity
    {
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public bool IsAvailbleForUrgentPurchase { get; set; }
        public string? ProductCategoryId { get; set; }

        [ForeignKey("ProductCategoryId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public ProductCategory ProductCategory { get; set; }
        public CurrencyTypeEnum CurrencyType { get; set; }
        public string VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }

        //[Column(TypeName = "decimal(18,2)")]
        //public decimal OriginalPrice { get; set; }

        //[Column(TypeName = "decimal(18,2)")]
        //public decimal WaivedPrice { get; set; }
        //public string ProductCode { get; set; }

        //public string Description { get; set; }
        //public int StockQuantity { get; set; }
        //public virtual ICollection<CustomerOrderItem> OrderItems { get; set; }
    }
}
