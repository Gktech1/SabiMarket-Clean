using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CustomerPurchaseDto
    {
        public string WaivedProductId { get; set; }
        public string? DeliveryInfo { get; set; }
        public string? ProofOfPayment { get; set; }
    }
}
