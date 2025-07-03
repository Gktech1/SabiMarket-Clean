using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Responses
{
    // Main dashboard DTO that matches the UI/UX
    public class DashboardReportDto
    {
        // Top cards
        public MarketCountDto MarketCount { get; set; }
        public TotalRevenueDto TotalRevenue { get; set; }
        // Levy Payments breakdown graph
        public LevyPaymentsBreakdownDto LevyPayments { get; set; }
        // Bottom cards
        public ComplianceRatesDto ComplianceRates { get; set; }
        public LevyCollectionPerMarketDto LevyCollection { get; set; }
        // Current date/time
        public DateTime CurrentDateTime { get; set; }
    }

    // Top left card - Market Count
    public class MarketCountDto
    {
        public int Count { get; set; }
        public string Description { get; set; } = "Total Number of registered markets";
    }

    // Top right card - Total Revenue
    public class TotalRevenueDto
    {
        public decimal Amount { get; set; }
        public TimeFrame TimeFrame { get; set; } = TimeFrame.ThisWeek;
        public string TimeFrameDisplay { get; set; } = "This week"; // For UI display
        public string Description { get; set; } = "Total levy paid";
    }

    // Center graph - Levy Payments Breakdown
    public class LevyPaymentsBreakdownDto
    {
        public List<string> Months { get; set; } // X-axis: Jan, Feb, Mar, etc.
        public List<MarketMonthlyDataDto> MarketData { get; set; }
    }

    public class MarketMonthlyDataDto
    {
        public string MarketName { get; set; }
        public string Color { get; set; } // For the line color (pink, turquoise, yellow)
        public List<decimal> Values { get; set; } // Y-axis values per month
    }

    // Bottom left - Compliance Rates
    public class ComplianceRatesDto
    {
        public int Year { get; set; }
        public List<MarketReportComplianceDto> MarketCompliance { get; set; }
    }

    public class MarketReportComplianceDto
    {
        public string MarketName { get; set; }
        public int CompliancePercentage { get; set; }
        public string Color { get; set; } // For the donut chart segment color
    }

    // Bottom right - Levy Collection Per Market
    public class LevyCollectionPerMarketDto
    {
        public int Year { get; set; }
        public decimal TotalAmount { get; set; }
        public List<MarketLevyDto> MarketLevy { get; set; }
    }

    public class MarketLevyDto
    {
        public string MarketName { get; set; }
        public decimal Amount { get; set; }
    }

    // Filter options
    public class FilterOptionsDto
    {
        public List<string> LGAs { get; set; }
        public List<string> Markets { get; set; }
        public List<int> Years { get; set; }
        public List<TimeFrameOption> TimeFrames { get; set; }
    }

    // Helper class for timeframe dropdown options
    public class TimeFrameOption
    {
        public TimeFrame Value { get; set; }
        public string Display { get; set; }
    }
}

