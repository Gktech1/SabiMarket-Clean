using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class ChairmanDashboardStatsDto
    {
        public int TotalTraders { get; set; }
        public int TotalCaretakers { get; set; }
        public decimal TotalLevies { get; set; }
        public decimal DailyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<LevyPaymentDetail> RecentLevyPayments { get; set; } = new List<LevyPaymentDetail>();
        public decimal PercentageChangeTraders { get; set; }
        public decimal PercentageChangeCaretakers { get; set; }
        public decimal PercentageChangeLevies { get; set; }
    }

    public class LevyPaymentDetail
    {
        public string PaymentId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaidBy { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymenPeriodEnum PaymentMethod { get; set; }
        public PaymenPeriodEnum PaymentPeriod { get; set; }
    }
}
