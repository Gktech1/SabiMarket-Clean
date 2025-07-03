using SabiMarket.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class ReportExportRequestDto
    {
        // Date Range Properties
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        // Market Selection
        public string? MarketId { get; set; } // Optional - if null means all markets

        // LGA Selection (from UI filter)
        public string? LGAId { get; set; } // Optional - if null means all LGAs

        // Year Selection (from UI filter)
        public int? Year { get; set; } // Optional - if null uses date range year

        // Report Type Selection
        [Required(ErrorMessage = "Report type is required")]
        public ReportType ReportType { get; set; }

        // Export Format
        [Required(ErrorMessage = "Export format is required")]
        public ExportFormat ExportFormat { get; set; }

        // Optional filtering parameters
        public bool IncludeComplianceRates { get; set; } = true;
        public bool IncludeRevenueBreakdown { get; set; } = true;
        public bool IncludeMarketMetrics { get; set; } = true;

        // Chart Options
        public bool IncludeCharts { get; set; } = true;

        // Time Zone
        public string TimeZone { get; set; } = "UTC";

        // Grouping Option (daily, weekly, monthly)
        public string GroupBy { get; set; } = "monthly";

        // Sorting Options
        public string SortBy { get; set; } = "revenue"; // Options: revenue, compliance, traders, etc.
        public bool SortDescending { get; set; } = true;

        // Limit Results
        public int? TopMarkets { get; set; } // Optional - only show top N markets if specified
    }
}