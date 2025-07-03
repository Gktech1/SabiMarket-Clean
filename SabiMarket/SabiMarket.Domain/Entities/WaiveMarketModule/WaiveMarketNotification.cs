using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class WaiveMarketNotification : BaseEntity
    {
        public string VendorId { get; set; }
        public string CustomerId { get; set; }
        public string Message { get; set; }
        public string? VendorResponse { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual Customer Customer { get; set; }
        //IsActive in BaseEntity flag will be used to set weather the message has been read or not
    }
}
