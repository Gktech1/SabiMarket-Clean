using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Domain.Entities.OrdersAndFeedback
{

    [Table("CustomerOrderItems")]
    public class CustomerOrderItem : BaseEntity
    {
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [ForeignKey("OrderId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual CustomerOrder Order { get; set; }

        [ForeignKey("ProductId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual WaivedProduct Product { get; set; }
    }

}
