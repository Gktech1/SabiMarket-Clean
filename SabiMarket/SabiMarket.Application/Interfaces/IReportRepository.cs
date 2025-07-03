using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Enum;

public interface IReportRepository : IGeneralRepository<Report>
{
    Task<Report> GetDashboardSummary();
    Task<Report> GetDailyMetricsAsync(DateTime date);
    Task<Report> GetMetricsAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Report>> GetLevyPaymentsBreakdown(int year);
    Task<Report> GetMarketComplianceRates(string marketId);
    Task<IEnumerable<Report>> GetLevyCollectionPerMarket();
    Task<Report> ExportReport(DateTime startDate, DateTime endDate);
    Task<DashboardReportDto> GetDashboardReportDataAsync(
          string lgaFilter = null,
          string marketFilter = null,
          int? year = null,
          TimeFrame timeFrame = TimeFrame.ThisWeek);
    Task<FilterOptionsDto> GetFilterOptionsAsync();
    Task<Report> ExportAdminReport(
            DateTime startDate,
            DateTime endDate,
            string marketId = null,
            string lgaId = null,
            string timeZone = "UTC");
}