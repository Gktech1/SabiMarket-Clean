using SabiMarket.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateLevyFrequencyDto
    {
        public string TraderIdentificationNumber { get; set; }
        public string MarketId { get; set; }
        public PaymentPeriodEnum PaymentFrequency { get; set; }
        public decimal Amount { get; set; }
    }
}
