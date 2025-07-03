using SabiMarket.Domain.Enum;

public class ChairmanLevyDashboardDto
{
    public decimal TotalLeviesCollected { get; set; }
    public decimal DailyLeviesCollected { get; set; }
    public decimal WeeklyLeviesCollected { get; set; }
    public decimal MonthlyLeviesCollected { get; set; }
    public int TotalTradersPaid { get; set; }
    public decimal PercentageChangeLevies { get; set; }
    public List<LevyPaymentDetailDto> RecentLevyPayments { get; set; } = new List<LevyPaymentDetailDto>();
}

public class LevyPaymentDetailDto
{
    public string PaymentId { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaidBy { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; }
}