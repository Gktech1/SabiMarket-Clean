using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Enum;
using System.Linq.Expressions;

public interface ITraderRepository : IGeneralRepository<Trader>
{
    void AddTrader(Trader trader);
    void AddBuildingTypeTrader(TraderBuildingType trader);
    void UpdateTrader(Trader trader);
    Task<Trader> GetByIdWithInclude(string traderId,
            params Expression<Func<Trader, object>>[] includes);

    Task<Trader> GetByIdWithIncludes(string traderId);
    Task<Trader> GetTraderById(string traderId, bool trackChanges);
    Task<Trader> GetTraderByTin(string tin, bool trackChanges);
    Task<Trader> GetTraderDetails(string userId);
    Task<int> GetTraderCountAsync(DateTime startDate, DateTime endDate);
    Task<PaginatorDto<IEnumerable<Trader>>> GetTradersByMarketAsync(string marketId, PaginationFilter paginationFilter, bool trackChanges = false);
    Task<IEnumerable<Trader>> GetAllTradersByMarketAsync(string marketId, bool trackChanges = false);
    IQueryable<Trader> GetTradersByCaretakerId(string caretakerId, bool trackChanges = false);
    Task<IEnumerable<Trader>> SearchTradersByQRCodeAsync(string qrCode, string goodBoyId);
    Task<int> GetTraderCountByGoodBoyIdAsync(string goodBoyId);
    void DeleteTrader(Trader trader);
    Task<Trader> GetTraderByIdAsync(string traderId, bool trackChanges);
    Task<int> GetDistinctTraderBuildingTypesCount(string traderId);
    Task<Trader> GetTraderByIdWithDetailsAsync(string traderId, bool trackChanges);
    Task<bool> TinExistsAsync(string tin, params string[] excludeTraderIds);
    Task<Trader> GetTraderByTinAsync(string tin, bool trackChanges = false);
    Task<TraderBuildingType> GetBuildingTypeByIdAsync(string traderId, bool trackChanges);
}