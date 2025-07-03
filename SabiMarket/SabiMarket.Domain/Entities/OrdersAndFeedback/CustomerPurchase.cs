using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Domain.Entities.OrdersAndFeedback
{
    public class CustomerPurchase : BaseEntity
    {
        public string WaivedProductId { get; set; }
        public string? DeliveryInfo { get; set; }
        public string? ProofOfPayment { get; set; }
        public bool IsPaymentConfirmed { get; set; }
        public WaivedProduct WaivedProduct { get; set; }
    }
}
