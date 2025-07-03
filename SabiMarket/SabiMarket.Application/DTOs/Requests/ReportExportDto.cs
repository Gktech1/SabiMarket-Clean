namespace SabiMarket.Application.DTOs.Requests
{
    public class ReportExportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Market Statistics
        public int TotalMarkets { get; set; }
        public List<MarketSummary> MarketDetails { get; set; }

        // Financial Metrics
        public decimal TotalRevenue { get; set; }
        public decimal DailyAverageRevenue { get; set; }
        public int TotalTransactions { get; set; }

        // Trader Metrics
        public int TotalTraders { get; set; }
        public int ActiveTraders { get; set; }
        public decimal TraderComplianceRate { get; set; }

        // Caretaker Metrics
        public int TotalCaretakers { get; set; }
        public int ActiveCaretakers { get; set; }

        // Payment Statistics
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; }
        public Dictionary<string, int> TransactionsByMarket { get; set; }

        // NEW: Monthly Revenue Data (for line charts)
        public List<MarketMonthlyRevenue> RevenueByMonth { get; set; }

        // NEW: LGA Information
        public List<LGASummary> LGADetails { get; set; }

        // NEW: Compliance Data by Market (for pie charts)
        public List<MarketCompliance> ComplianceByMarket { get; set; }

        public class MarketSummary
        {
            public string MarketId { get; set; }
            public string MarketName { get; set; }
            public string Location { get; set; }
            public string LGAName { get; set; }
            public int TotalTraders { get; set; }
            public decimal Revenue { get; set; }
            public decimal ComplianceRate { get; set; }
            public int TransactionCount { get; set; }
        }

        public class MarketMonthlyRevenue
        {
            public string MarketId { get; set; }
            public string MarketName { get; set; }
            public List<MonthlyData> MonthlyData { get; set; }

            // Helper properties for chart generation
            public List<string> Labels => MonthlyData?.Select(m => m.Month).ToList();
            public List<decimal> Values => MonthlyData?.Select(m => m.Revenue).ToList();
        }

        public class MonthlyData
        {
            public string Month { get; set; }  // "Jan", "Feb", etc.
            public int MonthNumber { get; set; }  // 1-12
            public int Year { get; set; }
            public decimal Revenue { get; set; }
            public int TransactionCount { get; set; }
        }

        public class LGASummary
        {
            public string LGAId { get; set; }
            public string LGAName { get; set; }
            public int MarketCount { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal ComplianceRate { get; set; }
        }

        public class MarketCompliance
        {
            public string MarketId { get; set; }
            public string MarketName { get; set; }
            public decimal CompliancePercentage { get; set; }
            public int CompliantTraders { get; set; }
            public int NonCompliantTraders { get; set; }
        }
    }
}
    /* public class ReportExportDto
     {
         public DateTime StartDate { get; set; }
         public DateTime EndDate { get; set; }

         // Market Statistics
         public int TotalMarkets { get; set; }
         public List<MarketSummary> MarketDetails { get; set; }

         // Financial Metrics
         public decimal TotalRevenue { get; set; }
         public decimal DailyAverageRevenue { get; set; }
         public int TotalTransactions { get; set; }

         // Trader Metrics
         public int TotalTraders { get; set; }
         public int ActiveTraders { get; set; }
         public decimal TraderComplianceRate { get; set; }

         // Caretaker Metrics
         public int TotalCaretakers { get; set; }
         public int ActiveCaretakers { get; set; }

         // Payment Statistics
         public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; }
         public Dictionary<string, int> TransactionsByMarket { get; set; }

         public class MarketSummary
         {
             public string MarketName { get; set; }
             public string Location { get; set; }
             public int TotalTraders { get; set; }
             public decimal Revenue { get; set; }
             public decimal ComplianceRate { get; set; }
             public int TransactionCount { get; set; }
         }
     }*/

