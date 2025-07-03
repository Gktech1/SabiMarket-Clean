using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateLevyRequestDto
    {
        public string MarketId { get; set; }
        public string TraderId { get; set; }
        public string GoodBoyId { get; set; }
        public decimal Amount { get; set; }
        public PaymentPeriodEnum? Period { get; set; }
        public PaymenPeriodEnum? PaymentMethod { get; set; }
        public PaymentStatusEnum? PaymentStatus { get; set; }
        public bool HasIncentive { get; set; }
        public decimal? IncentiveAmount { get; set; }
        public string Notes { get; set; }
    }


}
