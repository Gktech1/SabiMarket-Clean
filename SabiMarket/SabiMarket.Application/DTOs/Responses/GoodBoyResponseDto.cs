using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class GoodBoyResponseDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public string TraderId { get; set; }
        public string TraderOccupancy { get; set; }
        public string PaymentFrequency { get; set; }
        public decimal Amount { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public string DefaultPassword { get; set; } 
        public ICollection<GoodBoyLevyPaymentResponseDto> LevyPayments { get; set; }
    }
}
