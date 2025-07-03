using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using System.Linq.Expressions;

namespace SabiMarket.Infrastructure.Repositories
{
    public class TraderRepository : GeneralRepository<Trader>, ITraderRepository
    {
        private readonly ApplicationDbContext _context;

        public TraderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddTrader(Trader trader) => Create(trader);

        public void AddBuildingTypeTrader(TraderBuildingType trader)
        {
            _context.TraderBuildingTypes.Add(trader);
        }

        public void UpdateTrader(Trader trader) => Update(trader);

        /* public async Task<IEnumerable<Trader>> GetAllAssistCenterOfficer(bool trackChanges) =>
             await FindAll(trackChanges).ToListAsync();*/

        // New method to get trader by ID with custom includes

        /*public async Task<Trader> GetByIdWithInclude(string traderId,
            params Expression<Func<Trader, object>>[] includes)
        {
            var query = FindByCondition(t => t.Id == traderId, trackChanges: false)
                                        .Include(c => c.Chairman)
                                        .Include(lp => lp.LevyPayments);

            // Apply each include expression
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync();
        }*/

        public async Task<Trader> GetByIdWithInclude(string traderId,
    params Expression<Func<Trader, object>>[] includes)
        {
            // Use IQueryable<Trader> instead of the specific IIncludableQueryable type
            IQueryable<Trader> query = FindByCondition(t => t.Id == traderId, trackChanges: false)
                                      .Include(c => c.Chairman)
                                      .Include(lp => lp.LevyPayments);

            // Apply each include expression
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync();
        }


        public async Task<Trader> GetByIdWithIncludes(string traderId)
        {
            var query = FindByCondition(t => t.Id == traderId, trackChanges: false)
                                                      .Include(c => c.Chairman);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Trader> GetTraderByTinAsync(string tin, bool trackChanges = false)
        {
            var query = trackChanges
                ? _context.Traders.AsTracking()
                : _context.Traders.AsNoTracking();

            return await query
                .Where(t => t.TIN == tin)
                .Include(t => t.User)
                .Include(t => t.Market)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> TinExistsAsync(string tin, params string[] excludeTraderIds)
        {
            var query = _context.Traders.AsNoTracking()
                .Where(t => t.TIN == tin);

            if (excludeTraderIds?.Any() == true)
            {
                query = query.Where(t => !excludeTraderIds.Contains(t.Id));
            }

            return await query.AnyAsync();
        }

        /* public async Task<Trader> GetByIdWithInclude(string traderId)
         {
             var query = FindByCondition(t => t.Id == traderId, trackChanges: false)
                                 .Include(c => c.chairma);

             return await query.FirstOrDefaultAsync();
         }
 */
       /* public async Task<Trader> GetTraderByIdWithDetailsAsync(string traderId, bool trackChanges)
        {
            var query = _context.Traders
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .Where(t => t.Id == traderId);

            return trackChanges ?
                await query.FirstOrDefaultAsync() :
                await query.AsNoTracking().FirstOrDefaultAsync();
        }*/

        // Option 4: If you want to get distinct building type IDs (in case there are duplicates)
        public async Task<int> GetDistinctTraderBuildingTypesCount(string traderId)
        {
            var distinctCount = await _context.TraderBuildingTypes
                .Where(tbt => tbt.TraderId == traderId)
                .Select(tbt => tbt.Id) // Assuming BuildingTypeId is the property name
                .Distinct()
                .CountAsync();
            return distinctCount;
        }

        public async Task<TraderBuildingType> GetBuildingTypeByIdAsync(string traderId, bool trackChanges)
        {
            var query = _context.TraderBuildingTypes.Where(t => t.TraderId == traderId);

            return trackChanges ?
                await query.FirstOrDefaultAsync() :
                await query.AsNoTracking().FirstOrDefaultAsync();
        }
        public async Task<Trader> GetTraderByIdAsync(string traderId, bool trackChanges)
        {
            var query = _context.Traders.Where(t => t.Id == traderId);

            return trackChanges ?
                await query.FirstOrDefaultAsync() :
                await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Trader>> GetAllTradersByMarketAsync(
    string marketId,
    bool trackChanges = false)
        {
            return await FindByCondition(t => t.MarketId == marketId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.BuildingTypes)  // ADDED
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        /*public async Task<Trader> GetTraderById(string traderId, bool trackChanges) =>
                await FindByCondition(t => t.Id == traderId, trackChanges)
                    .Include(t => t.User)
                    .Include(t => t.Market)
                    .FirstOrDefaultAsync();*/

        /*public async Task<Trader> GetTraderDetails(string userId) =>
            await FindByCondition(t => t.UserId == userId, trackChanges: false)
                .Include(t => t.User)
                .Include(t => t.Market)
                .FirstOrDefaultAsync();*/

        // Get trader count by GoodBoy ID for dashboard
        public async Task<int> GetTraderCountByGoodBoyIdAsync(string goodBoyId)
        {
            // Get the market ID that the GoodBoy is assigned to
            var goodBoy = await _context.GoodBoys
                .AsNoTracking()
                .FirstOrDefaultAsync(gb => gb.Id == goodBoyId);

            if (goodBoy == null)
                return 0;

            // Count traders in that market
            return await _context.Traders
                .Where(t => t.MarketId == goodBoy.MarketId)
                .CountAsync();
        }

        // Search traders by QR code
        public async Task<IEnumerable<Trader>> SearchTradersByQRCodeAsync(string qrCode, string goodBoyId)
        {
            // First get the goodboy to find their market
            var goodBoy = await _context.GoodBoys
                .AsNoTracking()
                .FirstOrDefaultAsync(gb => gb.Id == goodBoyId);

            if (goodBoy == null)
                return new List<Trader>();

            // Get traders with the QR code in the same market as the goodboy
            return await FindByCondition(
                    t => t.QRCode == qrCode && t.MarketId == goodBoy.MarketId,
                    trackChanges: false)
                .Include(t => t.User)
                .Include(t => t.Market)
                .ToListAsync();
        }

        public async Task<Trader> GetTraderByIdWithDetailsAsync(string traderId, bool trackChanges)
        {
            var query = _context.Traders
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.BuildingTypes)  // ADDED
                .Include(t => t.LevyPayments)
                .Where(t => t.Id == traderId);

            return trackChanges ?
                await query.FirstOrDefaultAsync() :
                await query.AsNoTracking().FirstOrDefaultAsync();
        }

        // Update GetTraderById method
        public async Task<Trader> GetTraderById(string traderId, bool trackChanges) =>
            await FindByCondition(t => t.Id == traderId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.BuildingTypes)  // ADDED
                .FirstOrDefaultAsync();

        public async Task<Trader> GetTraderByTin(string tin, bool trackChanges) =>
          await FindByCondition(t => t.TIN == tin, trackChanges)
              .Include(t => t.User)
              .Include(t => t.Market)
              .Include(t => t.BuildingTypes)  // ADDED
              .FirstOrDefaultAsync();

        // Update GetTraderDetails method
        public async Task<Trader> GetTraderDetails(string userId) =>
            await FindByCondition(t => t.UserId == userId, trackChanges: false)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.BuildingTypes)  // ADDED
                .FirstOrDefaultAsync();

        public async Task<int> GetTraderCountAsync(DateTime startDate, DateTime endDate) =>
            await FindByCondition(t =>
                t.CreatedAt >= startDate &&
                t.CreatedAt <= endDate,
                trackChanges: false)
                .CountAsync();

        public async Task<PaginatorDto<IEnumerable<Trader>>> GetTradersByMarketAsync(
            string marketId,
            PaginationFilter paginationFilter,
            bool trackChanges = false)
        {
            var query = FindByCondition(t => t.MarketId == marketId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        /*public async Task<Trader> GetTraderByMarketAsync(
     string marketId, string userId,
     bool trackChanges = false)
        {
            // Using FirstOrDefaultAsync to return a single Trader or null
            var trader = await FindByCondition(t => t.MarketId == marketId && t.UserId == userId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            return trader;
        }*/

       /* public async Task<IEnumerable<Trader>> GetAllTradersByMarketAsync(
            string marketId,
            bool trackChanges = false)
        {
            return await FindByCondition(t => t.MarketId == marketId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }*/
        public IQueryable<Trader> GetTradersByCaretakerId(string caretakerId, bool trackChanges = false)
        {
            return FindByCondition(t => t.CaretakerId == caretakerId, trackChanges)
                .Include(t => t.User)
                .Include(t => t.Market)
                .Include(t => t.LevyPayments)
                .OrderByDescending(t => t.CreatedAt);
        }

        public void DeleteTrader(Trader trader)
        {
            Delete(trader);
        }
    }
}