namespace SabiMarket.Application.DTOs.Responses
{
    public class LevyResponseDto
    {
        public string Id { get; set; }  // Add this line
        public string ChairmanId { get; set; }
        public string TraderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentPeriod { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionReference { get; set; }
        public bool HasIncentive { get; set; }
        public decimal? IncentiveAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Notes { get; set; }
        public string GoodBoyId { get; set; }
        public DateTime CollectionDate { get; set; }
        public string QRCodeScanned { get; set; }
    }
}