using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;

namespace SabiMarket.Application.IRepositories
{
    public interface IMarketRepository
    {
        IQueryable<Market> GetMarketsQuery();
        void AddMarket(Market market);
        void DeleteMarket(Market market);
        // Task<IEnumerable<Market>> GetAllMarketForExport(bool trackChanges);
        Task<IEnumerable<Market>> GetAllMarketForExport(bool trackChanges, string searchQuery = null);
        Task<Market> GetMarketById(string id, bool trackChanges);
        Task<Market> GetMarketByUserId(string userId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Market>>> GetPagedMarket(PaginationFilter paginationFilter);
        Task<PaginatorDto<IEnumerable<Market>>> SearchMarket(string searchString, PaginationFilter paginationFilter);
        Task<Market> GetMarketByIdAsync(string marketId, bool trackChanges);
        Task<Market> GetMarketRevenueAsync(string marketId, DateTime startDate, DateTime endDate);
        Task<Market> GetComplianceRatesAsync(string marketId);
        IQueryable<Market> GetMarketsByCaretakerId(string caretakerId);

    }
}
