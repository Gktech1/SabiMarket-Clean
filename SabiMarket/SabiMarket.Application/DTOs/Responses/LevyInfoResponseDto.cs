using System;

namespace SabiMarket.Application.DTOs.Responses
{
    public class LevyInfoResponseDto
    {
        public string Id { get; set; }

        // Market Information
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public string MarketAddress { get; set; }
        public string MarketType { get; set; }

        // Levy Configuration
        public string TraderOccupancy { get; set; }  // e.g., "Open space", "Kiosk"
        public int PaymentFrequencyDays { get; set; }
        public decimal Amount { get; set; }

        // Audit Information
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Stats (Optional - for dashboard/reporting)
        public int ActiveTradersCount { get; set; }
        public int PaidTradersCount { get; set; }
        public int DefaultersTradersCount { get; set; }
        public decimal ExpectedDailyRevenue { get; set; }
        public decimal ActualDailyRevenue { get; set; }
    }
}