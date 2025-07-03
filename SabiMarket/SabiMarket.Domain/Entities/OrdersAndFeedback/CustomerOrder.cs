using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.OrdersAndFeedback
{
    [Table("CustomerOrders")]
    public class CustomerOrder : BaseEntity
    {
        public string CustomerId { get; set; }
        public string VendorId { get; set; }
        public DateTime OrderDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public OrderStatusEnum Status { get; set; }
        public string DeliveryAddress { get; set; }
        public string Notes { get; set; }
        
        [ForeignKey("CustomerId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Customer Customer { get; set; }

        [ForeignKey("VendorId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Vendor Vendor { get; set; }

        public virtual ICollection<CustomerOrderItem> OrderItems { get; set; } = new List<CustomerOrderItem>();
    }

}
