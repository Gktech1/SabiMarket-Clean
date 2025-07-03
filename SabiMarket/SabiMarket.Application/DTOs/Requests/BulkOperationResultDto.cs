// Add these DTOs to support the new Advertisement Service features

using SabiMarket.Application.DTOs.Advertisement;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Advertisement
{
    // 1. BULK OPERATIONS DTOs
    public class BulkOperationResultDto
    {
        public List<BulkOperationItemResult> Successful { get; set; } = new List<BulkOperationItemResult>();
        public List<BulkOperationItemResult> Failed { get; set; } = new List<BulkOperationItemResult>();
        public int TotalProcessed => Successful.Count + Failed.Count;
        public int SuccessCount => Successful.Count;
        public int FailureCount => Failed.Count;
        public double SuccessRate => TotalProcessed > 0 ? (double)SuccessCount / TotalProcessed * 100 : 0;
    }

    public class BulkOperationItemResult
    {
        public string Id { get; set; }
        public string Error { get; set; }
        public bool IsSuccess => string.IsNullOrEmpty(Error);
    }

    public class BulkApprovalRequestDto
    {
        public List<string> AdvertisementIds { get; set; } = new List<string>();
    }

    public class BulkRejectionRequestDto
    {
        public List<string> AdvertisementIds { get; set; } = new List<string>();
        [Required]
        public string Reason { get; set; }
    }

    // 2. ANALYTICS DTOs
    public class AdvertisementAnalyticsDto
    {
        public int TotalAdvertisements { get; set; }
        public int ActiveAdvertisements { get; set; }
        public int PendingAdvertisements { get; set; }
        public int RejectedAdvertisements { get; set; }
        public int ArchivedAdvertisements { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal DailyRevenue { get; set; }
        public int TotalViews { get; set; }
        public double AverageViewsPerAd { get; set; }
        public Dictionary<string, int> AdvertisementsByLocation { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AdvertisementsByPlacement { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByLocation { get; set; } = new Dictionary<string, decimal>();
        public List<TopPerformingAdDto> TopPerformingAds { get; set; } = new List<TopPerformingAdDto>();
        public List<DailyStatsDto> DailyStats { get; set; } = new List<DailyStatsDto>();
    }

    public class TopPerformingAdDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string VendorName { get; set; }
        public int Views { get; set; }
        public decimal Revenue { get; set; }
        public double ClickThroughRate { get; set; }
        public string Location { get; set; }
        public string Placement { get; set; }
    }

    public class DailyStatsDto
    {
        public DateTime Date { get; set; }
        public int NewAdvertisements { get; set; }
        public int Approvals { get; set; }
        public int Rejections { get; set; }
        public decimal Revenue { get; set; }
        public int TotalViews { get; set; }
    }

    public class RevenueAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyGrowth { get; set; }
        public decimal AverageOrderValue { get; set; }
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> RevenueByLocation { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> RevenueByPlacement { get; set; } = new Dictionary<string, decimal>();
        public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = new List<MonthlyRevenueDto>();
        public List<TopVendorRevenueDto> TopVendors { get; set; } = new List<TopVendorRevenueDto>();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int AdCount { get; set; }
        public decimal GrowthPercentage { get; set; }
    }

    public class TopVendorRevenueDto
    {
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorEmail { get; set; }
        public decimal TotalRevenue { get; set; }
        public int AdCount { get; set; }
        public decimal AverageAdValue { get; set; }
    }

    public class AnalyticsFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }
        public string Placement { get; set; }
        public string VendorId { get; set; }
        public string Status { get; set; }
    }

    // 3. DASHBOARD STATS DTOs
    public class AdvertisementDashboardStatsDto
    {
        public int TotalAdvertisements { get; set; }
        public int TodayAdvertisements { get; set; }
        public int WeekAdvertisements { get; set; }
        public int MonthAdvertisements { get; set; }
        public int PendingApprovals { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal DailyRevenue { get; set; }
        public int ActiveVendors { get; set; }
        public int TotalViews { get; set; }
        public double ConversionRate { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
        public List<AlertDto> Alerts { get; set; } = new List<AlertDto>();
        public Dictionary<string, int> StatusBreakdown { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByLocation { get; set; } = new Dictionary<string, decimal>();
    }

    public class RecentActivityDto
    {
        public string Id { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } // Created, Approved, Rejected, etc.
        public string AdvertisementId { get; set; }
        public string AdvertisementTitle { get; set; }
    }

    public class AlertDto
    {
        public string Id { get; set; }
        public string Type { get; set; } // Warning, Error, Info, Success
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Priority { get; set; } // High, Medium, Low
        public string ActionUrl { get; set; }
    }

    // 4. EXPORT DTOs
    public class AdvertisementExportRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string VendorId { get; set; }
        public ExportFormat Format { get; set; } = ExportFormat.Excel;
        public bool IncludeViews { get; set; } = true;
        public bool IncludePaymentDetails { get; set; } = true;
        public bool IncludeVendorDetails { get; set; } = true;
    }

    public enum ExportFormat
    {
        Excel,
        CSV,
        PDF
    }

    // 5. VENDOR MANAGEMENT DTOs
    public class VendorAdvertisementSummaryDto
    {
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string VendorEmail { get; set; }
        public string VendorPhone { get; set; }
        public int TotalAdvertisements { get; set; }
        public int ActiveAdvertisements { get; set; }
        public int PendingAdvertisements { get; set; }
        public int RejectedAdvertisements { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageAdValue { get; set; }
        public DateTime LastAdCreated { get; set; }
        public DateTime VendorJoinDate { get; set; }
        public bool IsActive { get; set; }
        public double SuccessRate { get; set; }
        public List<AdvertisementSummaryDto> RecentAdvertisements { get; set; } = new List<AdvertisementSummaryDto>();
    }

    public class BulkRejectAdvertisementsRequest
    {
        public List<string> AdvertisementIds { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }

    public class BulkApproveAdvertisementsRequest
    {
        public List<string> AdvertisementIds { get; set; } = new();
    }

    public class AdvertisementSummaryDto
    {
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string VendorId { get; set; }

        public string VendorName { get; set; }

        public string VendorEmail { get; set; }

        public string Status { get; set; }

        public string PaymentStatus { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        public string Location { get; set; }

        public string AdvertPlacement { get; set; }

        public string Language { get; set; }

        public string ImageUrl { get; set; }

        public string TargetUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int ViewCount { get; set; }

        // Calculated Properties
        public int DaysActive => DateTime.UtcNow > StartDate ? (int)(DateTime.UtcNow - StartDate).TotalDays : 0;

        public int DaysRemaining => EndDate > DateTime.UtcNow ? (int)(EndDate - DateTime.UtcNow).TotalDays : 0;

        public bool IsExpiring => DaysRemaining <= 7 && DaysRemaining > 0;

        public bool IsOverdue => EndDate < DateTime.UtcNow;

        public bool IsActive => Status == "Active";

        // Admin Information
        public string AdminId { get; set; }

        public string AdminName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string RejectionReason { get; set; }

        // Payment Information
        public bool HasPaymentProof => !string.IsNullOrEmpty(PaymentProofUrl);

        public string PaymentProofUrl { get; set; }

        public string BankTransferReference { get; set; }

        // Performance Metrics
        public double ClickThroughRate { get; set; }

        public double ConversionRate { get; set; }

        public int UniqueViews { get; set; }

        public int Clicks { get; set; }

        // Financial
        public decimal Revenue => PaymentStatus == "Verified" ? Price : 0;

        public decimal PendingRevenue => PaymentStatus == "Pending" || PaymentStatus == "Pending Verification" ? Price : 0;

        // Status Indicators
        public string StatusBadgeColor => Status switch
        {
            "Active" => "success",
            "Pending" => "warning",
            "Rejected" => "danger",
            "Archived" => "secondary",
            "Completed" => "info",
            _ => "light"
        };

        public string PaymentStatusBadgeColor => PaymentStatus switch
        {
            "Verified" => "success",
            "Pending" => "warning",
            "Pending Verification" => "info",
            "Rejected" => "danger",
            _ => "light"
        };

        // Urgency Indicators
        public string UrgencyLevel
        {
            get
            {
                if (IsOverdue) return "High";
                if (IsExpiring) return "Medium";
                if (Status == "Pending" && (DateTime.UtcNow - CreatedAt).TotalDays > 7) return "Medium";
                if (PaymentStatus == "Pending Verification" && (DateTime.UtcNow - (UpdatedAt ?? CreatedAt)).TotalDays > 3) return "High";
                return "Low";
            }
        }

        // Display Helpers
        public string FormattedPrice => Price.ToString("C");

        public string FormattedRevenue => Revenue.ToString("C");

        public string DurationDisplay => $"{(EndDate - StartDate).TotalDays:F0} days";

        public string ViewsDisplay => ViewCount > 1000 ? $"{ViewCount / 1000.0:F1}K" : ViewCount.ToString();

        public string CreatedAtDisplay => CreatedAt.ToString("MMM dd, yyyy");

        public string LastUpdatedDisplay => UpdatedAt?.ToString("MMM dd, yyyy") ?? "Never";

        // Progress Indicators
        public double CompletionPercentage
        {
            get
            {
                if (StartDate > DateTime.UtcNow) return 0;
                if (EndDate < DateTime.UtcNow) return 100;

                var totalDays = (EndDate - StartDate).TotalDays;
                var elapsedDays = (DateTime.UtcNow - StartDate).TotalDays;

                return totalDays > 0 ? Math.Min(100, (elapsedDays / totalDays) * 100) : 0;
            }
        }

        // Additional Metadata
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        // Quick Actions Available
        public List<string> AvailableActions
        {
            get
            {
                var actions = new List<string>();

                if (Status == "Pending")
                {
                    actions.AddRange(new[] { "Approve", "Reject", "Edit" });
                }
                else if (Status == "Active")
                {
                    actions.AddRange(new[] { "Archive", "View Performance" });
                }
                else if (Status == "Archived")
                {
                    actions.AddRange(new[] { "Restore", "Delete" });
                }

                if (PaymentStatus == "Pending Verification")
                {
                    actions.AddRange(new[] { "Verify Payment", "Reject Payment" });
                }

                actions.AddRange(new[] { "View Details", "Download Report" });

                return actions.Distinct().ToList();
            }
        }
    }


    public class VendorFilterDto
    {
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? JoinedFrom { get; set; }
        public DateTime? JoinedTo { get; set; }
        public decimal? MinRevenue { get; set; }
        public decimal? MaxRevenue { get; set; }
        public int? MinAds { get; set; }
        public int? MaxAds { get; set; }
    }

    // 6. PAYMENT VERIFICATION DTOs
    public class PaymentVerificationDto
    {
        public string Id { get; set; }
        public string AdvertisementId { get; set; }
        public string AdvertisementTitle { get; set; }
        public string VendorName { get; set; }
        public string ActionRequired { get; set; }
        public string ActionUrl { get; set; }
    }

    // 9. PERFORMANCE DTOs
    public class AdvertisementPerformanceDto
    {
        public string AdvertisementId { get; set; }
        public string Title { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViews { get; set; }
        public int Clicks { get; set; }
        public double ClickThroughRate { get; set; }
        public decimal Revenue { get; set; }
        public double ConversionRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysActive { get; set; }
        public double AverageViewsPerDay { get; set; }
        public double AverageClicksPerDay { get; set; }
        public string Location { get; set; }
        public string Placement { get; set; }
        public List<DailyPerformanceDto> DailyPerformance { get; set; } = new List<DailyPerformanceDto>();
        public List<HourlyPerformanceDto> HourlyPerformance { get; set; } = new List<HourlyPerformanceDto>();
        public Dictionary<string, int> ViewsByLocation { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ViewsByDevice { get; set; } = new Dictionary<string, int>();
    }

    public class DailyPerformanceDto
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
        public int Clicks { get; set; }
        public double ClickThroughRate { get; set; }
        public decimal Revenue { get; set; }
    }

    public class HourlyPerformanceDto
    {
        public int Hour { get; set; }
        public int Views { get; set; }
        public int Clicks { get; set; }
        public double ClickThroughRate { get; set; }
    }

    // 10. ENHANCED ADVERTISEMENT RESPONSE DTOs
    public class EnhancedAdvertisementResponseDto : AdvertisementResponseDto
    {
        public string VendorName { get; set; }
        public string VendorEmail { get; set; }
        public string VendorPhone { get; set; }
        public string AdminName { get; set; }
        public string AdminEmail { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectionReason { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViews { get; set; }
        public int Clicks { get; set; }
        public double ClickThroughRate { get; set; }
        public double ConversionRate { get; set; }
        public bool HasPaymentProof { get; set; }
        public DateTime? PaymentSubmittedAt { get; set; }
        public DateTime? PaymentVerifiedAt { get; set; }
        public int DaysToExpiry { get; set; }
        public bool IsExpiringSoon { get; set; }
        public bool IsOverdue { get; set; }
        public List<AdvertisementHistoryDto> History { get; set; } = new List<AdvertisementHistoryDto>();
    }

    public class AdvertisementHistoryDto
    {
        public string Id { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string PerformedBy { get; set; }
        public string PerformedByRole { get; set; }
        public DateTime Timestamp { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Notes { get; set; }
    }

    // 11. STATISTICAL SUMMARY DTOs
    public class AdvertisementStatsSummaryDto
    {
        public string Period { get; set; } // Today, This Week, This Month, This Year
        public int NewAdvertisements { get; set; }
        public int ApprovedAdvertisements { get; set; }
        public int RejectedAdvertisements { get; set; }
        public int ActiveAdvertisements { get; set; }
        public int ExpiredAdvertisements { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageAdvertisementValue { get; set; }
        public int TotalViews { get; set; }
        public double AverageViewsPerAdvertisement { get; set; }
        public double ApprovalRate { get; set; }
        public double RejectionRate { get; set; }
        public int NewVendors { get; set; }
        public int ActiveVendors { get; set; }
        public decimal RevenueGrowth { get; set; }
        public double ViewGrowth { get; set; }
    }

    // 12. NOTIFICATION DTOs
    public class AdvertisementNotificationDto
    {
        public string Id { get; set; }
        public string Type { get; set; } // NewAdvertisement, PaymentSubmitted, ExpiringAd, etc.
        public string Title { get; set; }
        public string Message { get; set; }
        public string Priority { get; set; } // High, Medium, Low
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string TargetUserId { get; set; }
        public string TargetUserRole { get; set; }
        public string AdvertisementId { get; set; }
        public string ActionUrl { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    // 13. BATCH PROCESSING DTOs
    public class BatchProcessRequestDto
    {
        public List<string> AdvertisementIds { get; set; } = new List<string>();
        public string Action { get; set; } // Approve, Reject, Archive, Delete, etc.
        public string Reason { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class BatchProcessResponseDto
    {
        public string BatchId { get; set; }
        public string Status { get; set; } // Processing, Completed, Failed, PartialSuccess
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int SuccessfulItems { get; set; }
        public int FailedItems { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<BatchProcessItemResult> Results { get; set; } = new List<BatchProcessItemResult>();
        public string ErrorMessage { get; set; }
        public double ProgressPercentage { get; set; }
    }

    public class BatchProcessItemResult
    {
        public string AdvertisementId { get; set; }
        public string AdvertisementTitle { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string Status { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    // 14. AUDIT AND COMPLIANCE DTOs
    public class AdvertisementAuditDto
    {
        public string AdvertisementId { get; set; }
        public string AdvertisementTitle { get; set; }
        public string VendorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string CurrentStatus { get; set; }
        public List<AdvertisementAuditLogDto> AuditTrail { get; set; } = new List<AdvertisementAuditLogDto>();
        public List<string> ComplianceIssues { get; set; } = new List<string>();
        public bool IsCompliant { get; set; }
        public string ComplianceScore { get; set; }
    }

    public class AdvertisementAuditLogDto
    {
        public string Id { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public string PerformedByRole { get; set; }
        public DateTime Timestamp { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }

    // 15. COMPARISON AND BENCHMARKING DTOs
    public class AdvertisementComparisonDto
    {
        public List<AdvertisementPerformanceDto> Advertisements { get; set; } = new List<AdvertisementPerformanceDto>();
        public AdvertisementBenchmarkDto Benchmark { get; set; }
        public Dictionary<string, object> ComparisonMetrics { get; set; } = new Dictionary<string, object>();
    }

    public class AdvertisementBenchmarkDto
    {
        public double AverageViews { get; set; }
        public double AverageClickThroughRate { get; set; }
        public double AverageConversionRate { get; set; }
        public decimal AverageRevenue { get; set; }
        public string Industry { get; set; }
        public string Location { get; set; }
        public string Placement { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}

// 7. ADMIN FILTERING DTOs
public class AdminAdvertisementFilterDto : AdvertisementFilterRequestDto
{
    public string VendorId { get; set; }
    public string VendorName { get; set; }
    public string VendorEmail { get; set; }
    public string PaymentStatus { get; set; }
    public DateTime? PaymentSubmittedFrom { get; set; }
    public DateTime? PaymentSubmittedTo { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int? MinViews { get; set; }
    public int? MaxViews { get; set; }
    public string AdminId { get; set; }
    public string ApprovalStatus { get; set; }
}

// 8. ALERT DTOs
public class AdvertisementAlertDto
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string AdvertisementId { get; set; }
    public string AdvertisementTitle { get; set; }
    public string ActionUrl { get; set; }
    public string ActionRequired { get; set; }
        
}




