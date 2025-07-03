using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Domain.Entities.OrdersAndFeedback
{

    [Table("CustomerFeedbacks")]
    public class CustomerFeedback : BaseEntity
    {
        public string CustomerId { get; set; }
        public string VendorId { get; set; }
        public string VendorCode { get; set; }
        public string Comment { get; set; }
        public string? ImageUrl { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("CustomerId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Customer Customer { get; set; }

        [ForeignKey("VendorId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Vendor Vendor { get; set; }
    }
}
