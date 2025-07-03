namespace SabiMarket.Application.DTOs.Responses
{
    public class ReportMetricsDto
    {
        // Market Statistics (shown in top-left card)
        public int MarketCount { get; set; }
        public string MarketCountLabel { get; set; } = "Number of registered markets";

        // Revenue Statistics (shown in top-right card)
        public decimal TotalRevenueGenerated { get; set; }
        public string Period { get; set; }  // e.g., "This week", "This month"

        // Levy Payments Breakdown (shown in center graph)
        public ICollection<MarketLevyBreakdownDto> LevyBreakdown { get; set; }

        // Compliance Statistics (shown in bottom-left)
        public decimal TotalComplianceRate { get; set; }
        public ICollection<MarketComplianceRateDto> MarketComplianceRates { get; set; }

        // Revenue Per Market (shown in bottom-right)
        public decimal TotalRevenue { get; set; }
        public ICollection<MarketRevenueBreakdownDto> MarketRevenues { get; set; }

        public DateTime ReportGeneratedAt { get; set; }
    }

    // For the center graph showing levy payments over time
    public class MarketLevyBreakdownDto
    {
        public string MarketName { get; set; }
        public ICollection<MonthlyLevyDto> MonthlyData { get; set; }
    }

    public class MonthlyLevyDto
    {
        public string Month { get; set; }  // Jan, Feb, etc.
        public decimal Amount { get; set; }
    }

    // For the market compliance breakdown
    public class MarketComplianceRateDto
    {
        public string MarketName { get; set; }
        public decimal CompliancePercentage { get; set; }
    }

    // For the revenue per market breakdown
    public class MarketRevenueBreakdownDto
    {
        public string MarketName { get; set; }
        public decimal Amount { get; set; }
    }

    public class DailyMetricsDto
    {
        // Dashboard card metrics (shown in Image 2)
        public TradersMetricsDto Traders { get; set; }
        public CaretakersMetricsDto Caretakers { get; set; }
        public LeviesMetricsDto Levies { get; set; }
    }

    public class TradersMetricsDto
    {
        public int Total { get; set; }
        public decimal PercentageChange { get; set; }  // e.g., 8.5%
        public string ChangeDirection { get; set; }    // "Up" or "Down"
        public string TimePeriod { get; set; }        // e.g., "from yesterday"
    }

    public class CaretakersMetricsDto
    {
        public int Total { get; set; }
        public decimal PercentageChange { get; set; }
        public string ChangeDirection { get; set; }
        public string TimePeriod { get; set; }
    }

    public class LeviesMetricsDto
    {
        public decimal Total { get; set; }
        public decimal PercentageChange { get; set; }
        public string ChangeDirection { get; set; }
        public string TimePeriod { get; set; }
    }

    public class MarketRevenueDto
    {
        // Market Information
        public string MarketId { get; set; }
        public string MarketName { get; set; }

        // Revenue Statistics
        public decimal TotalRevenue { get; set; }
       // public ICollection<DailyRevenueDto> DailyRevenue { get; set; }

        // Payment Methods Statistics
        public ICollection<PaymentMethodStatDto> PaymentMethods { get; set; }

        // Trends
        public decimal GrowthRate { get; set; }
        public decimal AverageDaily { get; set; }
        public decimal AverageMonthly { get; set; }
    }

    public class PaymentMethodStatDto
    {
        public string Method { get; set; }  // Cash, Bank Transfer, etc.
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }

    public class MarketComplianceDto
    {
        // Market Information
        public string MarketId { get; set; }
        public string MarketName { get; set; }

        // Compliance Overview
        public decimal OverallComplianceRate { get; set; }

        // Trader Statistics
        public int TotalTraders { get; set; }
        public int CompliantTraders { get; set; }
        public int NonCompliantTraders { get; set; }

        // Compliance by Category
        public ICollection<ComplianceCategoryDto> ComplianceByCategory { get; set; }

        // Trend Analysis
        //public ICollection<ComplianceTrendDto> MonthlyTrends { get; set; }
    }

    public class ComplianceCategoryDto
    {
        public string Category { get; set; }  // e.g., "Shop", "Kiosk", "Open Space"
        public decimal ComplianceRate { get; set; }
        public int TotalInCategory { get; set; }
        public int CompliantInCategory { get; set; }
    }
}