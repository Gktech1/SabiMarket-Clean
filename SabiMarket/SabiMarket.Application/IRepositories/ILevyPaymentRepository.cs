using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Enum;

public interface ILevyPaymentRepository : IGeneralRepository<LevyPayment>
{
    void AddPayment(LevyPayment levyPayment);
    Task<LevyPayment> GetByIdAsync(string id, bool trackChanges = false);
    IQueryable<LevyPayment> GetPaymentsQuery();
    Task<LevySetup> GetLevySetupById(string id, bool trackChanges);
    Task<IEnumerable<LevyPayment>> GetAllLevyPaymentForExport(bool trackChanges);
    Task<LevyPayment> GetPaymentById(string id, bool trackChanges);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPayment(int? period, PaginationFilter paginationFilter);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPaymentsAsync(string chairmanId, PaginationFilter paginationFilter, bool trackChanges);
    Task<LevyPayment> GetLevySetupByMarketAndFrequency(string marketId, PaymentPeriodEnum paymentFrequency, bool trackChanges = false);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchPayment(string searchString, PaginationFilter paginationFilter);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchLevyPaymentsInMarket(
    string marketId,
    string searchQuery,
    PaginationFilter paginationFilter,
    bool trackChanges);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
       PaymentPeriodEnum? period,
       string? searchQuery,
       PaginationFilter paginationFilter,
       bool trackChanges = false);
    Task<decimal> GetTotalLeviesAsync(DateTime startDate, DateTime endDate);
    void DeleteLevyPayment(LevyPayment levy);
    Task<decimal> GetTotalRevenueAsync();
    Task<IEnumerable<LevyPayment>> GetAllLevySetupsAsync(bool trackChanges);
    Task<IEnumerable<LevySetup>> GetAllLevySetups(bool trackChanges);
    Task<IEnumerable<LevyPayment>> GetByMarketAndOccupancyAsync(string marketId, MarketTypeEnum traderOccupancy);
    Task<LevyPayment> GetMarketLevySetup(string marketId, PaymentPeriodEnum period);
    Task<IQueryable<LevyPayment>> GetMarketLevySetups(string marketId);
    Task<IEnumerable<LevyPayment>> GetLevyPaymentsByTraderIdAsync(string traderId);
    //Task<IEnumerable<LevyPayment>> GetLevyPaymentsByDateRangeAsync(string goodBoyId, DateTime fromDate, DateTime toDate);

    Task<IEnumerable<GoodBoyLevyPaymentResponseDto>> GetLevyPaymentsByDateRangeAsync(string goodBoyId, DateTime fromDate, DateTime toDate);

    Task<IEnumerable<LevyPayment>> GetByMarketAndOccupancy(string marketId, MarketTypeEnum traderOccupancy);

    Task<IEnumerable<LevyPayment>> GetLevyPaymentsByTraderIdAndDateRangeAsync(
           string traderId,
           DateTime fromDate,
           DateTime toDate);
    Task<decimal> GetTotalLevyAmountByGoodBoyIdAsync(string goodBoyId, DateTime fromDate, DateTime toDate);
    Task<PaginatorDto<IEnumerable<LevyPayment>>> GetTodayLeviesForGoodBoyAsync(
            string goodBoyId,
            PaginationFilter paginationFilter,
            bool trackChanges = false);
    /*Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPaymentsByDateRange(
    string goodBoyId,
    PaginationFilter paginationFilter,
    bool trackChanges = false);*/
    Task<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>> GetLevyPaymentsByDateRange(
    string goodBoyId,
    PaginationFilter paginationFilter,
    bool trackChanges = false);
    Task<IEnumerable<LevyPayment>> GetRecentLevyPaymentsByTraderIdAsync(
            string traderId,
            int limit = 10);
    Task<LevyPayment> GetLatestLevyPaymentByTraderIdAsync(string traderId);
    Task<decimal> GetTotalLevyAmountByTraderIdAsync(string traderId);
    Task<LevyPayment> GetLatestActiveLevyForTrader(string traderId);
    Task<List<LevyPayment>> GetRecentPaymentsForTrader(string traderId, int count);

    Task<IEnumerable<LevyPayment>> GetTraderPaymentHistory(string traderId, bool excludeSetupRecords = true);
    //Task<LevyPayment> GetActiveLevySetupByMarketAndOccupancy(string marketId, MarketTypeEnum occupancyType);
    Task<IEnumerable<LevyPayment>> GetActiveLevySetupsByMarket(string marketId);
    //Task<LevyPayment> GetLevySetupByPaymentFrequency(PaymentPeriodEnum paymentFrequency);

    Task<LevySetup> GetLevtSetupByIdAsync(string id, bool trackChanges = false);

    Task<LevySetup> GetLevySetupByPaymentFrequency(PaymentPeriodEnum paymentFrequency);
    Task<LevySetup> GetActiveLevySetupByMarketAndOccupancy(string marketId, MarketTypeEnum occupancyType);
    Task<LevyPayment> GetActiveLevySetupByMarketAndOccupancyAsync(string marketId, MarketTypeEnum occupancyType);

    Task<IEnumerable<LevySetup>> GetByMarketAndOccupancies(string marketId, MarketTypeEnum traderOccupancy);
    Task<IEnumerable<LevyPayment>> GetActiveLevySetupsByMarket(string marketId, bool trackChanges = false);

    Task<IEnumerable<LevyPayment>> GetActiveSetupRecordsByTraderIdAsync(string traderId, bool trackChanges = false);
    void AddLevelSetup(LevySetup levySetup);
    void UpdateLevelSetup(LevySetup levySetup);

    void DeleteLevySetup(LevySetup levy);
}