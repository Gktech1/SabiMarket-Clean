using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("Reports")]
public class Report : BaseEntity
{
    [Required]
    public int MarketCount { get; set; }
    [Required]
    public decimal TotalRevenueGenerated { get; set; }
    public DateTime ReportDate { get; set; }
    // For levy payments breakdown
    public string MarketId { get; set; }
    public string MarketName { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    // For compliance rates
    public decimal ComplianceRate { get; set; }
    public int TotalTraders { get; set; }
    public int CompliantTraders { get; set; }
    // For levy collection per market
    public decimal TotalLevyCollected { get; set; }

    // Payment methods analysis
    [NotMapped]
    public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new Dictionary<string, decimal>();

    // Market details collection
    [NotMapped]
    public List<MarketDetail> MarketDetails { get; set; } = new List<MarketDetail>();

    // Monthly revenue data for charts
    [NotMapped]
    public List<MonthlyRevenue> MonthlyRevenueData { get; set; } = new List<MonthlyRevenue>();

    // Daily average calculation
    [NotMapped]
    public decimal DailyAverageRevenue { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PaymentTransactions { get; set; }
    public int ActiveMarkets { get; set; }
    public int NewTradersCount { get; set; }
    public bool IsDaily { get; set; }
    public int TotalCaretakers { get; set; }

    // Navigation property
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Market Market { get; set; }
}

// Add these classes within the same namespace
[NotMapped]
public class MarketDetail
{
    public string MarketId { get; set; }
    public string MarketName { get; set; }
    public string Location { get; set; }
    public int TotalTraders { get; set; }
    public decimal Revenue { get; set; }
    public decimal ComplianceRate { get; set; }
    public int TransactionCount { get; set; }
}

[NotMapped]
public class MonthlyRevenue
{
    public string MarketId { get; set; }
    public string MarketName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}