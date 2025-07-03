public class DashboardMetricsResponseDto
{
    public MetricChangeDto Traders { get; set; }
    public MetricChangeDto Caretakers { get; set; }
    public MetricChangeDto Levies { get; set; }
    public string TimePeriod { get; set; }
    public MetricChangeDto ComplianceRate { get; set; }
    public MetricChangeDto TransactionCount { get; set; }
    public MetricChangeDto ActiveMarkets { get; set; }
}

public class MetricChangeDto
{
    public decimal CurrentValue { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal PercentageChange { get; set; }
    public string ChangeDirection { get; set; }
}