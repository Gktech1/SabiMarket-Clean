using iText.Commons.Actions.Contexts;
using Mailjet.Client.Resources;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using System.Linq.Expressions;

namespace SabiMarket.Infrastructure.Repositories
{
    public class ChairmanRepository : GeneralRepository<Chairman>, IChairmanRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public ChairmanRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext; 
        }

        public async Task<Chairman> GetChairmanByIdAsync(string userId, bool trackChanges)
        {
            return await FindByCondition(c => c.LocalGovernmentId == userId, trackChanges)
                .Include(c => c.Market) 
                .FirstOrDefaultAsync();
        }

        /* public async Task<Chairman> GetChairmanById(string userId, bool trackChanges)
         {
             return await FindByCondition(c => c.Id == userId, trackChanges)
                 .Include(c => c.Market)
                 .FirstOrDefaultAsync();
         }*/

        public async Task<Chairman> GetChairmanByChairmanIdId(string chairmaId, bool trackChanges)
        {
            // Store the query type properly with a var to avoid explicit typing
            var query = FindByCondition(c => c.UserId == chairmaId, trackChanges);

            // Apply includes
            var queryWithIncludes = query
                .Include(c => c.Market)
                .Include(c => c.User)
                .Include(c => c.LocalGovernment);

            // Apply AsNoTracking if needed
            var finalQuery = trackChanges
                ? queryWithIncludes
                : queryWithIncludes.AsNoTracking();

            // Execute the query
            return await finalQuery.FirstOrDefaultAsync();
        }

        public async Task<Chairman> GetChairmanByUserId(string userId, bool trackChanges) =>
         await FindByCondition(x => x.UserId == userId, trackChanges)
             .Include(a => a.User)  // Include the User entity
             .Include(a => a.Market)
             .Include(c => c.LocalGovernment)
             .FirstOrDefaultAsync();
        public async Task<Chairman> GetChairmanById(string UserId, bool trackChanges)
        {
            // Store the query type properly with a var to avoid explicit typing
            var query = FindByCondition(c => c.UserId == UserId, trackChanges);

            // Apply includes
            var queryWithIncludes = query
                .Include(c => c.Market)
                .Include(c => c.User)
                .Include(c => c.LocalGovernment);

            // Apply AsNoTracking if needed
            var finalQuery = trackChanges
                ? queryWithIncludes
                : queryWithIncludes.AsNoTracking();

            // Execute the query
            return await finalQuery.FirstOrDefaultAsync();
        }
        public async Task<Chairman> GetChairmanByMarketIdAsync(string marketId, bool trackChanges)
        {
            return await FindByCondition(c => c.MarketId == marketId, trackChanges)
                .Include(c => c.Market)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatorDto<IEnumerable<Chairman>>> GetChairmenWithPaginationAsync(
      PaginationFilter paginationFilter, bool trackChanges, string? searchTerm)
        {
            // Get the base query, assuming FindByCondition works as a base filter
            var query = FindByCondition(
                expression: _ => true,  // Initially return all Chairmen (no specific filter)
                trackChanges: trackChanges
            );

            // Apply eager loading for related entities
            query = query.Include(c => c.User)  // Include User details
                         .Include(c => c.Market) // Include Market details
                         .Include(c => c.LocalGovernment) // Include Local Government details
                         .OrderBy(c => c.CreatedAt);  // Apply sorting by creation date

            // Apply search filter if a search term is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    (c.User.FirstName.Contains(searchTerm) ||
                     c.User.LastName.Contains(searchTerm) ||
                     c.User.Email.Contains(searchTerm) ||
                     c.Market.MarketName.Contains(searchTerm) ||
                     c.LocalGovernment.Name.Contains(searchTerm)));
            }

            // Apply pagination (this returns a Task, so await it)
            var result = await query.Paginate(paginationFilter);

            return result;
        }

        public async Task<PaginatorDto<IEnumerable<Chairman>>> GetChairmenWithPaginationAsync(
      PaginationFilter paginationFilter, bool trackChanges, string? searchTerm, string userId)
        {
            // Get the base query, assuming FindByCondition works as a base filter
            var query = FindByCondition(
                expression: _ => true,  // Initially return all Chairmen (no specific filter)
                trackChanges: trackChanges
            );

            // Apply eager loading for related entities
            query = query.Where(c => c.UserId == userId)
                         .Include(c => c.User)  // Include User details
                         .Include(c => c.Market) // Include Market details
                         .Include(c => c.LocalGovernment) // Include Local Government details
                         .OrderBy(c => c.CreatedAt);  // Apply sorting by creation date

            // Apply search filter if a search term is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    (c.User.FirstName.Contains(searchTerm) ||
                     c.User.LastName.Contains(searchTerm) ||
                     c.User.Email.Contains(searchTerm) ||
                     c.Market.MarketName.Contains(searchTerm) ||
                     c.LocalGovernment.Name.Contains(searchTerm)));
            }

            // Apply pagination (this returns a Task, so await it)
            var result = await query.Paginate(paginationFilter);

            return result;
        }



        public async Task<IEnumerable<Chairman>> SearchChairmenAsync(
            string searchTerm, PaginationFilter paginationFilter, bool trackChanges)
        {
            return await FindByCondition(
                    c => c.FullName.Contains(searchTerm) || c.Email.Contains(searchTerm),
                    trackChanges)
                .OrderBy(c => c.FullName)
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToListAsync();
        }

        public async Task<bool> ChairmanExistsAsync(string userId, string marketId)
        {
            return await FindByCondition(
                c => c.UserId == userId && c.MarketId == marketId,
                trackChanges: false).AnyAsync();
        }


        public async Task<int> CountAsync(Expression<Func<Chairman, bool>> predicate)
        {
            return await _dbContext.Set<Chairman>()
                .Include(c => c.User)
                .Where(predicate)
                .CountAsync();
        }

        public async Task<bool> MarketHasChairmanAsync(string marketId, bool trackChanges)
        {
            return await FindByCondition(
                c => c.MarketId == marketId,
                trackChanges).AnyAsync();
        }

        public void CreateChairman(Chairman chairman) => Create(chairman);
       
        public void DeleteChairman(Chairman chairman) => Delete(chairman);  
       
        public void UpdateChairman(Chairman chairman) => Update(chairman);

        public async Task<IEnumerable<Chairman>> GetReportsByChairmanIdAsync(string chairmanId)
        {
            return await FindByCondition(r => r.Id == chairmanId, trackChanges: false)
                .ToListAsync();
        }


        public async Task<ChairmanDashboardStatsDto> GetChairmanDashboardStatsAsync(string chairmanId)
        {
            // Get the chairman with their market information
            var chairman = await FindByCondition(c => c.UserId == chairmanId, trackChanges: false)
                .Include(c => c.User)
                .Include(c => c.Market)
                .FirstOrDefaultAsync();

            var stats = new ChairmanDashboardStatsDto();

            if (chairman == null)
                return null;

            var marketId = chairman.MarketId;
            if (string.IsNullOrEmpty(marketId))
            {
                stats.PercentageChangeTraders = 0;
                stats.PercentageChangeCaretakers = 0;
                stats.PercentageChangeLevies = 0;
                stats.RecentLevyPayments = new List<LevyPaymentDetail>();
                stats.DailyRevenue = 0;
                stats.MonthlyRevenue = 0;   
                stats.TotalCaretakers = 0;  
                stats.TotalLevies = 0;  
                return stats;
            }
             

            var now = DateTime.UtcNow;
            var startOfDay = now.Date;
            var startOfWeek = startOfDay.AddDays(-(int)startOfDay.DayOfWeek);
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Ensure consistent casing for GUID comparison - normalize to uppercase
            var normalizedMarketId = marketId.ToUpper();

            // Create stats object
            stats = new ChairmanDashboardStatsDto
            {
                // Count total traders in the chairman's market - remove .Where() temporarily to debug
                TotalTraders = await _dbContext.Traders
                    .Where(t => t.MarketId.ToUpper() == normalizedMarketId)
                    .Where(t => t.MarketId.ToUpper() == normalizedMarketId)
                    .CountAsync(),

                // Count total caretakers in the chairman's market
                TotalCaretakers = await _dbContext.Caretakers
                    .Where(c => c.MarketId.ToUpper() == normalizedMarketId)
                    .CountAsync(),

                // Sum total levies collected in the chairman's market
                // Based on the SQL query result, we know there are records with this MarketId
                // The issue is likely with the PaymentStatus filter
                TotalLevies = await _dbContext.LevyPayments
                    .Where(l => l.MarketId.ToUpper() == normalizedMarketId)
                    // Adjust the PaymentStatus check - your DB shows numeric values
                    // Assuming that a status of "1" corresponds to PaymentStatusEnum.Paid
                    // If your PaymentStatus is stored as int in the database
                    .SumAsync(l => l.Amount),

                // Sum daily revenue - also try without payment status filter
                DailyRevenue = await _dbContext.LevyPayments
                    .Where(l => l.MarketId.ToUpper() == normalizedMarketId &&
                           l.PaymentDate >= startOfDay)
                    .SumAsync(l => l.Amount),

                // Sum weekly revenue
                WeeklyRevenue = await _dbContext.LevyPayments
                    .Where(l => l.MarketId.ToUpper() == normalizedMarketId &&
                           l.PaymentDate >= startOfWeek)
                    .SumAsync(l => l.Amount),

                // Sum monthly revenue
                MonthlyRevenue = await _dbContext.LevyPayments
                    .Where(l => l.MarketId.ToUpper() == normalizedMarketId &&
                           l.PaymentDate >= startOfMonth)
                    .SumAsync(l => l.Amount),

                // Get recent levy payments - include trader information for display
                RecentLevyPayments = await _dbContext.LevyPayments
                    .Where(l => l.MarketId.ToUpper() == normalizedMarketId)
                    .Include(l => l.Trader)
                        .ThenInclude(t => t.User)
                    .Include(l => l.GoodBoy)
                        .ThenInclude(g => g.User)
                    .OrderByDescending(l => l.PaymentDate)
                    .Take(10)
                    .Select(l => new LevyPaymentDetail
                    {
                        PaymentId = l.Id,
                        AmountPaid = l.Amount,
                        PaidBy = l.Trader != null ? l.Trader.User.FirstName + " " + l.Trader.User.LastName :
                                 l.GoodBoy != null ? l.GoodBoy.User.FirstName + " " + l.GoodBoy.User.LastName : "Unknown",
                        PaymentDate = l.PaymentDate,
                        PaymentMethod = l.PaymentMethod,
                        PaymentPeriod = l.PaymentMethod
                    })
                    .ToListAsync()
            };

            // After initial query, check for issues with PaymentStatus enum matching
            if (stats.TotalLevies == 0)
            {
                // Check if any levy payments exist at all
                var anyLevyPayments = await _dbContext.LevyPayments
                    .AnyAsync(l => l.MarketId.ToUpper() == normalizedMarketId);

                // Get raw payment status values to check if they match your enum
                var rawStatusValues = await _dbContext.LevyPayments
                    .Where(l => l.MarketId.ToUpper() == normalizedMarketId)
                    .Select(l => new { StatusValue = (int)l.PaymentStatus, StatusName = l.PaymentStatus.ToString() })
                    .Distinct()
                    .ToListAsync();
                /*
                                _logger.LogInformation($"Payment status values in system: {string.Join(", ", rawStatusValues.Select(s => $"{s.StatusName}({s.StatusValue})"))}");

                                // Manually check what enum value corresponds to "Paid" status
                                _logger.LogInformation($"PaymentStatusEnum.Paid value: {(int)PaymentStatusEnum.Paid}");*/
            }

            // Calculate percentage changes from previous day
            var yesterdayStart = startOfDay.AddDays(-1);
            var yesterdayEnd = startOfDay;

            var yesterdayTraders = await _dbContext.Traders
                .Where(t => t.MarketId.ToUpper() == normalizedMarketId && t.CreatedAt < yesterdayEnd)
                .CountAsync();

            var yesterdayCaretakers = await _dbContext.Caretakers
                .Where(c => c.MarketId.ToUpper() == normalizedMarketId && c.CreatedAt < yesterdayEnd)
                .CountAsync();

            var yesterdayLevies = await _dbContext.LevyPayments
                .Where(l => l.MarketId.ToUpper() == normalizedMarketId &&
                       l.PaymentDate >= yesterdayStart &&
                       l.PaymentDate < yesterdayEnd)
                .SumAsync(l => l.Amount);

            // Calculate percentage changes
            stats.PercentageChangeTraders = CalculatePercentageChange(yesterdayTraders, stats.TotalTraders);
            stats.PercentageChangeCaretakers = CalculatePercentageChange(yesterdayCaretakers, stats.TotalCaretakers);
            stats.PercentageChangeLevies = CalculatePercentageChange(yesterdayLevies, stats.TotalLevies);

            return stats;
        }


        /*     public async Task<ChairmanDashboardStatsDto> GetChairmanDashboardStatsAsync(string chairmanId)
             {
                 // Get the chairman with their market information
                 var chairman = await FindByCondition(c => c.UserId == chairmanId, trackChanges: false)
                     .Include(c => c.User)
                     .Include(c => c.Market)
                     .FirstOrDefaultAsync();

                 if (chairman == null)
                     return null;

                 var marketId = chairman.MarketId;
                 if (string.IsNullOrEmpty(marketId))
                     return null;

                 var now = DateTime.UtcNow;
                 var startOfDay = now.Date;
                 var startOfWeek = startOfDay.AddDays(-(int)startOfDay.DayOfWeek);
                 var startOfMonth = new DateTime(now.Year, now.Month, 1);

                 // Create stats object
                 var stats = new ChairmanDashboardStatsDto
                 {
                     // Count total traders in the chairman's market
                     TotalTraders = await _dbContext.Traders
                         .Where(t => t.MarketId == marketId)
                         .CountAsync(),

                     // Count total caretakers in the chairman's market
                     TotalCaretakers = await _dbContext.Caretakers
                         .Where(c => c.MarketId == marketId)
                         .CountAsync(),

                     // Sum total levies collected in the chairman's market where payment status is successful
                     TotalLevies = await _dbContext.LevyPayments
                         .Where(l => l.MarketId == marketId && l.PaymentStatus == PaymentStatusEnum.Paid)
                         .SumAsync(l => l.Amount),

                     // Sum daily revenue
                     DailyRevenue = await _dbContext.LevyPayments
                         .Where(l => l.MarketId == marketId &&
                                l.PaymentDate >= startOfDay &&
                                l.PaymentStatus == PaymentStatusEnum.Paid)
                         .SumAsync(l => l.Amount),

                     // Sum weekly revenue
                     WeeklyRevenue = await _dbContext.LevyPayments
                         .Where(l => l.MarketId == marketId &&
                                l.PaymentDate >= startOfWeek &&
                                l.PaymentStatus == PaymentStatusEnum.Paid)
                         .SumAsync(l => l.Amount),

                     // Sum monthly revenue
                     MonthlyRevenue = await _dbContext.LevyPayments
                         .Where(l => l.MarketId == marketId &&
                                l.PaymentDate >= startOfMonth &&
                                l.PaymentStatus == PaymentStatusEnum.Paid)
                         .SumAsync(l => l.Amount),

                     // Get recent levy payments - include trader information for display
                     RecentLevyPayments = await _dbContext.LevyPayments
                         .Where(l => l.MarketId == marketId && l.PaymentStatus == PaymentStatusEnum.Paid)
                         .Include(l => l.Trader)
                         .OrderByDescending(l => l.PaymentDate)
                         .Take(10)
                         .Select(l => new LevyPaymentDetail
                         {
                             PaymentId = l.Id,
                             AmountPaid = l.Amount,
                             PaidBy = l.Trader != null ? l.Trader.User.FirstName + " " + l.Trader.User.LastName :
                                      l.GoodBoy != null ? l.GoodBoy.User.FirstName + " " + l.GoodBoy.User.LastName : "Unknown",
                             PaymentDate = l.PaymentDate,
                             PaymentMethod = l.PaymentMethod,
                             PaymentPeriod = l.PaymentMethod
                         })
                         .ToListAsync()
                 };

                 // Calculate percentage changes from previous day
                 var yesterdayStart = startOfDay.AddDays(-1);
                 var yesterdayEnd = startOfDay;

                 var yesterdayTraders = await _dbContext.Traders
                     .Where(t => t.MarketId == marketId && t.CreatedAt < yesterdayEnd)
                     .CountAsync();

                 var yesterdayCaretakers = await _dbContext.Caretakers
                     .Where(c => c.MarketId == marketId && c.CreatedAt < yesterdayEnd)
                     .CountAsync();

                 var yesterdayLevies = await _dbContext.LevyPayments
                     .Where(l => l.MarketId == marketId &&
                            l.PaymentDate >= yesterdayStart &&
                            l.PaymentDate < yesterdayEnd &&
                            l.PaymentStatus == PaymentStatusEnum.Paid)
                     .SumAsync(l => l.Amount);

                 // Calculate percentage changes
                 stats.PercentageChangeTraders = CalculatePercentageChange(yesterdayTraders, stats.TotalTraders);
                 stats.PercentageChangeCaretakers = CalculatePercentageChange(yesterdayCaretakers, stats.TotalCaretakers);
                 stats.PercentageChangeLevies = CalculatePercentageChange(yesterdayLevies, stats.TotalLevies);

                 return stats;
             }
     */
        private decimal CalculatePercentageChange(int previous, int current)
        {
            if (previous == 0)
                return current > 0 ? 100 : 0;

            return Math.Round(((decimal)(current - previous) / previous) * 100, 1);
        }

        private decimal CalculatePercentageChange(decimal previous, decimal current)
        {
            if (previous == 0)
                return current > 0 ? 100 : 0;

            return Math.Round(((current - previous) / previous) * 100, 1);
        }

    }
}
