using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Application.DTOs.Responses;
using System.Linq;

namespace SabiMarket.Infrastructure.Repositories
{
    public class LevyPaymentRepository : GeneralRepository<LevyPayment>, ILevyPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public LevyPaymentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddPayment(LevyPayment levyPayment) => Create(levyPayment);

        public void AddLevelSetup(LevySetup levySetup)
        {
            _context.LevySetups.Add(levySetup);
        }

        public void UpdateLevelSetup(LevySetup levySetup)
        {
            _context.LevySetups.Update(levySetup);
        }
        public IQueryable<LevyPayment> GetPaymentsQuery()
        {
            return FindAll(trackChanges: false)
                .Include(l => l.Market)
                .Include(l => l.Trader)
                .Include(l => l.GoodBoy);
        }
        public async Task<IEnumerable<LevyPayment>> GetAllLevyPaymentForExport(bool trackChanges) =>
            await FindAll(trackChanges).ToListAsync();

        public async Task<LevyPayment> GetPaymentById(string id, bool trackChanges) =>
            await FindByCondition(x => x.Id == id, trackChanges)
                .Include(x => x.Market)
                .Include(x => x.Trader)
                .FirstOrDefaultAsync();
        public async Task<LevySetup> GetLevySetupById(string id, bool trackChanges)
        {
            var query = trackChanges
               ? _context.LevySetups.AsTracking()
               : _context.LevySetups.AsNoTracking();
              
               return  await query.Where(x => x.Id == id)
              .FirstOrDefaultAsync();
        }


        public async Task<LevyPayment> GetLevySetupByMarketAndFrequency(string marketId, PaymentPeriodEnum paymentFrequency, bool trackChanges = false)
        {
            var query = trackChanges
                ? _context.LevyPayments.AsTracking()
                : _context.LevyPayments.AsNoTracking();

            return await query
                .Where(ls => ls.MarketId == marketId && ls.Period == paymentFrequency)
                .Include(ls => ls.Market)
                .OrderByDescending(ls => ls.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LevyPayment>> GetActiveSetupRecordsByTraderIdAsync(string traderId, bool trackChanges = false)
        {
            return await FindByCondition(
                lp => lp.TraderId == traderId &&
                      lp.IsSetupRecord == true &&
                      lp.IsActive == true,
                trackChanges)
                .Include(lp => lp.Market)
                .Include(lp => lp.Trader)
                    .ThenInclude(t => t.User)
                .OrderByDescending(lp => lp.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LevyPayment>> GetActiveLevySetupsByMarket(string marketId, bool trackChanges = false)
        {
            var query = trackChanges
                ? _context.LevyPayments.AsTracking()
                : _context.LevyPayments.AsNoTracking();

            return await query
                .Where(ls => ls.MarketId == marketId && ls.IsActive)
                .Include(ls => ls.Market)
                .OrderBy(ls => ls.Period)
                .ToListAsync();
        }


        /*  // Modified method to get levy configurations per market
          public async Task<IEnumerable<LevyPayment>> GetAllLevySetupsAsync(bool trackChanges)
          {
              return await _context.LevyPayments
                  .AsTracking(trackChanges ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
                  .Include(lp => lp.Market)
                  .GroupBy(lp => new { lp.MarketId, lp.Period })
                  .Select(g => g.OrderByDescending(lp => lp.CreatedAt).First())
                  .OrderBy(lp => lp.MarketId)
                  .ThenBy(lp => lp.Period)
                  .ToListAsync();
          }*/

        public async Task<IEnumerable<LevyPayment>> GetAllLevySetupsAsync(bool trackChanges)
        {
            return await _context.LevyPayments
                .AsTracking(trackChanges ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
                .Include(lp => lp.Market)
                .Where(lp => lp.CreatedAt == _context.LevyPayments
                    .Where(inner => inner.MarketId == lp.MarketId && inner.Period == lp.Period)
                    .Max(inner => inner.CreatedAt))
                .OrderBy(lp => lp.MarketId)
                .ThenBy(lp => lp.Period)
                .ToListAsync();
        }

        public async Task<IEnumerable<LevySetup>> GetAllLevySetups(bool trackChanges)
        {
            var query = await _context.LevySetups
                .AsTracking(trackChanges ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking)
                .Include(lp => lp.Market)
                .Where(lp => lp.CreatedAt == _context.LevySetups
                    .Where(inner => inner.MarketId == lp.MarketId && inner.PaymentFrequency == lp.PaymentFrequency)
                    .Max(inner => inner.CreatedAt))
                .OrderBy(lp => lp.MarketId)
                .ThenBy(lp => lp.PaymentFrequency)
                .ToListAsync();

            return query;
        }


        public async Task<LevySetup> GetLevySetupByPaymentFrequency(PaymentPeriodEnum paymentFrequency)
        {
            return await _context.LevySetups
                .Where(lp => lp.PaymentFrequency ==  paymentFrequency)
                .OrderBy(lp => lp.MarketId)
                .ThenBy(lp => lp.PaymentFrequency)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
     PaymentPeriodEnum? period,
     string searchQuery,
     PaginationFilter paginationFilter,
     bool trackChanges = false)
        {
            // Start building our query
            var baseQuery = _context.LevyPayments
                .AsTracking(trackChanges ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking);

            // Apply period filter if specified
            if (period.HasValue)
            {
                baseQuery = baseQuery.Where(l => l.Period == period.Value);
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                bool isNumeric = decimal.TryParse(searchQuery, out decimal searchAmount);

                baseQuery = baseQuery.Where(l =>
                    (l.TransactionReference != null && EF.Functions.Like(l.TransactionReference.ToLower(), $"%{searchQuery}%")) ||
                    (isNumeric && l.Amount == searchAmount) ||
                    (l.Notes != null && EF.Functions.Like(l.Notes.ToLower(), $"%{searchQuery}%"))
                );
            }

            // Apply ordering to the base query
            var orderedBaseQuery = baseQuery
                .OrderByDescending(p => p.PaymentDate)
                .ThenBy(p => p.PaymentStatus);

            // Use your existing pagination on the base query
            var paginatedPayments = await orderedBaseQuery.Paginate(paginationFilter);

            // Now load the related entities for the paginated results
            foreach (var payment in paginatedPayments.PageItems)
            {
                // Get trader with user if TraderId is not null
                if (!string.IsNullOrEmpty(payment.TraderId))
                {
                    var trader = await _context.Traders
                        .AsNoTracking()
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.Id == payment.TraderId);

                    payment.Trader = trader;


                    if (trader != null && trader.User == null)
                    {
                        // Try to fetch the user directly if it's missing
                        var user = await _context.Users.FindAsync(trader.UserId);
                        if (trader != null)
                        {
                            trader.User = user;
                        }
                    }
                }

                // Similar logic for GoodBoy
                if (!string.IsNullOrEmpty(payment.GoodBoyId))
                {
                    var goodBoy = await _context.GoodBoys
                        .AsNoTracking()
                        .Include(gb => gb.User)
                        .FirstOrDefaultAsync(gb => gb.Id == payment.GoodBoyId);

                    payment.GoodBoy = goodBoy;
                }

                // Also try to load data via ChairmanId since your data shows this relationship
                if (!string.IsNullOrEmpty(payment.ChairmanId) && (payment.Trader == null || payment.Trader.User == null))
                {
                    var chairman = await _context.Chairmen
                        .AsNoTracking()
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.Id == payment.ChairmanId);

                    if (chairman?.User != null)
                    {
                        // If we couldn't get trader info but chairman is available, create a temporary trader
                        // with chairman user data for display purposes
                        if (payment.Trader == null)
                        {
                            payment.Trader = new Trader { User = chairman.User };
                        }
                    }
                }
            }

            return paginatedPayments;
        }

        // Dashboard statistics for GoodBoy
        public async Task<decimal> GetTotalLevyAmountByGoodBoyIdAsync(string goodBoyId, DateTime fromDate, DateTime toDate)
        {
            return await FindByCondition(
                lp => lp.GoodBoyId == goodBoyId &&
                      lp.PaymentDate >= fromDate &&
                      lp.PaymentDate <= toDate &&
                      lp.PaymentStatus == PaymentStatusEnum.Paid,
                trackChanges: false)
                .SumAsync(lp => lp.Amount);
        }

        // Get today's levy payments for a GoodBoy

        // Add this method to your LevyPaymentRepository class
        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetTodayLeviesForGoodBoyAsync(
            string goodBoyId,
            PaginationFilter paginationFilter,
            bool trackChanges = false)
        {
            // Get today's date
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);

            // Debug info
            Console.WriteLine($"Searching for goodBoyId: {goodBoyId}, date: {today}");

            // Create the query
            var query = FindByCondition(
                lp => lp.GoodBoyId == goodBoyId &&
                      lp.PaymentStatus == PaymentStatusEnum.Paid &&
                      lp.PaymentDate >= today &&
                      lp.PaymentDate < tomorrow,
                trackChanges)
                .Include(lp => lp.Trader)
                    .ThenInclude(t => t.User)
                .Include(lp => lp.Market)
                .OrderByDescending(lp => lp.PaymentDate)
                .ThenByDescending(lp => lp.CreatedAt);

            // Apply pagination
            return await query.Paginate(paginationFilter);
        }

        public async Task<IEnumerable<GoodBoyLevyPaymentResponseDto>> GetLevyPaymentsByDateRangeAsync(string goodBoyId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                // First debug: Check levy payments without includes
                var levyPaymentsBasic = await FindByCondition(
                    lp => lp.GoodBoyId == goodBoyId &&
                          lp.PaymentDate >= fromDate &&
                          lp.PaymentDate <= toDate &&
                          lp.PaymentStatus == PaymentStatusEnum.Paid,
                    trackChanges: false)
                    .ToListAsync();

                Console.WriteLine($"Found {levyPaymentsBasic.Count} levy payments");
                foreach (var levy in levyPaymentsBasic.Take(5))
                {
                    Console.WriteLine($"LevyId: {levy.Id}, TraderId: {levy.TraderId}, GoodBoyId: {levy.GoodBoyId}");
                }

                // Second debug: Check trader IDs - Fixed to use TraderId instead of GoodBoyId
                var traderIds = levyPaymentsBasic.Select(l => l.GoodBoyId).Distinct().ToList();
                var tradersExist = await _context.Traders
                    .Where(t => traderIds.Contains(t.UserId)) // Using UserId based on your earlier finding
                    .ToListAsync();

                Console.WriteLine($"Found {tradersExist.Count} traders for these TraderId values");
                foreach (var traderDetail in tradersExist.Take(5))
                {
                    Console.WriteLine($"Trader - Id: {traderDetail.Id}, UserId: {traderDetail.UserId}, TraderName: {traderDetail.TraderName}");
                }

                var traders = tradersExist.FirstOrDefault();
                // Use join instead of Include for better performance
                var query = _context.LevyPayments
                    .Where(levy => levy.GoodBoyId == goodBoyId &&
                                   levy.PaymentDate >= fromDate &&
                                   levy.PaymentDate <= toDate &&
                                   levy.PaymentStatus == PaymentStatusEnum.Paid)
                    .GroupJoin(_context.Traders,
                              levy => levy.TraderId,
                              trader => trader.UserId, // Based on your earlier finding
                              (levy, traders) => new { levy, traders })
                    .SelectMany(x => x.traders.DefaultIfEmpty(),
                               (x, trader) => new GoodBoyLevyPaymentResponseDto
                               {
                                   Id = x.levy.Id,
                                   Amount = x.levy.Amount,
                                   PaymentDate = x.levy.PaymentDate,
                                   CreatedAt = x.levy.CreatedAt,
                                   Status = x.levy.PaymentStatus.ToString(),
                                   TraderName = traders != null ? traders.TraderName ?? "No Trader Name" : "No Trader Found"
                               })
                    .OrderByDescending(dto => dto.PaymentDate)
                    .ThenByDescending(dto => dto.CreatedAt);

                // Execute the query
                var result = await query.ToListAsync();

                // Debug the result
                Console.WriteLine($"Final result count: {result.Count}");
                foreach (var item in result.Take(5))
                {
                    Console.WriteLine($"LevyId: {item.Id}, TraderName: {item.TraderName}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetLevyPaymentsByDateRangeAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<LevyPayment> GetByIdAsync(string id, bool trackChanges = false)
        {
            return await FindByCondition(x => x.Id == id, trackChanges)
                .Include(x => x.Market)
                .Include(x => x.Trader)
                    .ThenInclude(t => t.User)
                .Include(x => x.Chairman)
                    .ThenInclude(c => c.User)
                .Include(x => x.GoodBoy)
                    .ThenInclude(gb => gb.User)
                .FirstOrDefaultAsync();
        }


        // FIXED CODE (CORRECT):
        public async Task<LevySetup> GetLevtSetupByIdAsync(string id, bool trackChanges = false)
        {
            var query = trackChanges
                ? _context.LevySetups.AsTracking()
                : _context.LevySetups.AsNoTracking();

            return await query
                .Where(x => x.Id == id)
                .Include(x => x.Chairman)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync();
        }

        /*  public async Task<IEnumerable<LevyPayment>> GetLevyPaymentsByDateRangeAsync(string goodBoyId, DateTime fromDate, DateTime toDate)
          {
              try
              {
                  // First debug: Check levy payments without includes
                  var levyPaymentsBasic = await FindByCondition(
                      lp => lp.GoodBoyId == goodBoyId &&
                            lp.PaymentDate >= fromDate &&
                            lp.PaymentDate <= toDate &&
                            lp.PaymentStatus == PaymentStatusEnum.Paid,
                      trackChanges: false)
                      .ToListAsync();

                  Console.WriteLine($"Found {levyPaymentsBasic.Count} levy payments");
                  foreach (var levy in levyPaymentsBasic.Take(5))
                  {
                      Console.WriteLine($"LevyId: {levy.Id}, TraderId: {levy.TraderId}, GoodBoyId: {levy.GoodBoyId}");
                  }

                  // Second debug: Check trader IDs
                  var traderIds = levyPaymentsBasic.Select(l => l.GoodBoyId).Distinct().ToList();
                  var tradersExist = await _context.Traders
                      .Where(t => traderIds.Contains(t.UserId)) // Using UserId based on your earlier finding
                      .ToListAsync();

                  Console.WriteLine($"Found {tradersExist.Count} traders for these TraderId values");
                  foreach (var trader in tradersExist.Take(5))
                  {
                      Console.WriteLine($"Trader - Id: {trader.Id}, UserId: {trader.UserId}, TraderName: {trader.TraderName}");
                  }

                  // Now try the full query with includes
                  *//*  var result = await FindByCondition(
                        lp => lp.GoodBoyId == goodBoyId &&
                              lp.PaymentDate >= fromDate &&
                              lp.PaymentDate <= toDate &&
                              lp.PaymentStatus == PaymentStatusEnum.Paid,
                        trackChanges: false)
                        .Include(lp => lp.Trader)
                            .ThenInclude(t => t.User)
                        .Include(lp => lp.Market)
                        .OrderByDescending(lp => lp.PaymentDate)
                        .AsNoTracking()
                        .ToListAsync();*//*

                  var query = _context.LevyPayments
                  .Include(levy => levy.Trader) // Include navigation property
                  .Where(levy => levy.GoodBoyId == goodBoyId &&
                                 levy.PaymentDate >= fromDate &&
                                 levy.PaymentDate <= toDate &&
                                 levy.PaymentStatus == PaymentStatusEnum.Paid)
                  .Select(levy => new GoodBoyLevyPaymentResponseDto
                  {
                      Id = levy.Id,
                      Amount = levy.Amount,
                      PaymentDate = levy.PaymentDate,
                      CreatedAt = levy.CreatedAt,
                      Status = levy.PaymentStatus.ToString(),
                      TraderName = levy.Trader != null ? levy.Trader.TraderName ?? "No Trader Name" : "No Trader Found"
                  })
                  .OrderByDescending(dto => dto.PaymentDate)
                  .ThenByDescending(dto => dto.CreatedAt);

                  // Execute the query
                  var result = await query.ToListAsync();

                  // Debug the result
                  Console.WriteLine($"Final result count: {result.Count}");
                  foreach (var item in result.Take(5))
                  {
                      Console.WriteLine($"LevyId: {item.Id}, TraderName: {item.TraderName}");
                  }

                  return result;
              }
              catch (Exception ex)
              {
                  Console.WriteLine($"Exception in GetLevyPaymentsByDateRangeAsync: {ex.Message}");
                  Console.WriteLine($"Stack trace: {ex.StackTrace}");
                  throw;
              }
          }*/

        /* public async Task<IEnumerable<LevyPayment>> GetLevyPaymentsByDateRangeAsync(string goodBoyId, DateTime fromDate, DateTime toDate)
         {
             return await FindByCondition(
                 lp => lp.GoodBoyId == goodBoyId &&
                       lp.PaymentDate >= fromDate &&
                       lp.PaymentDate <= toDate &&
                       lp.PaymentStatus == PaymentStatusEnum.Paid,
                 trackChanges: false)
                 .Include(lp => lp.Trader) // Include Trader
                     .ThenInclude(t => t.User) // Then Include User from Trader
                 .Include(lp => lp.Market)
                 .OrderByDescending(lp => lp.PaymentDate)
                 .AsNoTracking() // This can help with performance
                 .ToListAsync();
         }*/

        public async Task<IEnumerable<LevyPayment>> GetLevyPaymentsByTraderIdAndDateRangeAsync(
           string traderId,
           DateTime fromDate,
           DateTime toDate)
        {
            // Normalize date range to include the entire day
            var normalizedFromDate = fromDate.Date;
            var normalizedToDate = toDate.Date.AddDays(1).AddTicks(-1); // End of the toDate

            return await FindByCondition(
                lp => lp.TraderId == traderId &&
                      lp.PaymentDate >= normalizedFromDate &&
                      lp.PaymentDate <= normalizedToDate &&
                      lp.PaymentStatus == PaymentStatusEnum.Paid,
                trackChanges: false)
                .Include(lp => lp.Trader)
                    .ThenInclude(t => t.User)
                .Include(lp => lp.Market)
                .Include(lp => lp.GoodBoy)
                    .ThenInclude(gb => gb.User)
                .OrderByDescending(lp => lp.PaymentDate)
                .ThenByDescending(lp => lp.CreatedAt)
                .ToListAsync();
        }

        public async Task<LevySetup> GetActiveLevySetupByMarketAndOccupancy(string marketId, MarketTypeEnum occupancyType)
        {
            return await _context.LevySetups
                .Where(lp => lp.MarketId == marketId &&
                            lp.IsSetupRecord == true &&
                            lp.IsActive == true &&
                            (lp.OccupancyType == occupancyType || lp.OccupancyType == null))
                .OrderByDescending(lp => lp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<LevyPayment> GetActiveLevySetupByMarketAndOccupancyAsync(string marketId, MarketTypeEnum occupancyType)
        {
            return await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId &&
                            lp.IsSetupRecord == true &&
                            lp.IsActive == true &&
                            (lp.OccupancyType == occupancyType || lp.OccupancyType == null))
                .OrderByDescending(lp => lp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LevyPayment>> GetActiveLevySetupsByMarket(string marketId)
        {
            return await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId &&
                            lp.IsSetupRecord == true &&
                            lp.IsActive == true)
                .OrderByDescending(lp => lp.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LevyPayment>> GetTraderPaymentHistory(string traderId, bool excludeSetupRecords = true)
        {
            var query = _context.LevyPayments
                .Where(lp => lp.TraderId == traderId);

           /* if (excludeSetupRecords)
            {
                query = query.Where(lp => lp.IsSetupRecord == false || lp.IsSetupRecord == null);
            }*/

            return await query
                .OrderByDescending(lp => lp.PaymentDate)
                .ToListAsync();
        }

        /*public async Task<IEnumerable<LevyPayment>> GetByMarketAndOccupancy(string marketId, MarketTypeEnum occupancyType)
        {
            return await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId &&
                            (lp.OccupancyType == occupancyType || lp.OccupancyType == null))
                .ToListAsync();
        }*/

        public async Task<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>> GetLevyPaymentsByDateRange(
    string goodBoyId,
    PaginationFilter paginationFilter,
    bool trackChanges = false)
        {
            try
            {
                // First, let's check what levy payments we're getting
                var levyPayments = await _context.LevyPayments
                    .Where(levy => levy.GoodBoyId == goodBoyId &&
                                  levy.PaymentStatus == PaymentStatusEnum.Paid &&
                                  levy.Period == PaymentPeriodEnum.Daily)
                    .ToListAsync();

                Console.WriteLine($"Found {levyPayments.Count} levy payments");
                foreach (var levy in levyPayments.Take(5)) // Log first 5
                {
                    Console.WriteLine($"LevyId: {levy.Id}, TraderId: {levy.GoodBoyId}");
                }

                // Now let's check if we have traders with those IDs
                var traderIds = levyPayments.Select(l => l.GoodBoyId).Distinct().ToList();
                var traders = await _context.Traders
                    .Where(t => traderIds.Contains(t.UserId))
                    .ToListAsync();

                Console.WriteLine($"Found {traders.Count} traders");
                foreach (var traderdetail in traders.Take(5)) // Log first 5
                {
                    Console.WriteLine($"TraderId: {traderdetail.Id}, TraderName: {traderdetail.TraderName}");
                }
                var trader = traders.FirstOrDefault();

                // Now the actual query with Include to ensure navigation properties are loaded
                var query = _context.LevyPayments
                    .Include(levy => levy.Trader) // Ensure the navigation property is loaded
                    .Where(levy => levy.GoodBoyId == goodBoyId &&
                                  levy.PaymentStatus == PaymentStatusEnum.Paid &&
                                  levy.Period == PaymentPeriodEnum.Daily)
                    .Select(levy => new GoodBoyLevyPaymentResponseDto
                    {
                        Id = levy.Id,
                        Amount = levy.Amount,
                        PaymentDate = levy.PaymentDate,
                        CreatedAt = levy.CreatedAt,
                        Status = levy.PaymentStatus.ToString(),
                        TraderName = trader!.TraderName != null ? trader.TraderName : "No Trader Found"
                    })
                    .OrderByDescending(dto => dto.PaymentDate)
                    .ThenByDescending(dto => dto.CreatedAt);

                return await query.Paginate(paginationFilter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetLevyPaymentsByDateRange: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }


        // Get levy payments by trader ID
        public async Task<IEnumerable<LevyPayment>> GetLevyPaymentsByTraderIdAsync(string traderId)
        {
            return await FindByCondition(
                lp => lp.TraderId == traderId,
                trackChanges: false)
                .Include(lp => lp.Trader)
                .Include(lp => lp.Market)
                .Include(lp => lp.GoodBoy)
                    .ThenInclude(gb => gb.User)
                .OrderByDescending(lp => lp.PaymentDate)
                .ToListAsync();
        }

      /*  // Create levy payment
        public async Task CreateLevyPaymentAsync(LevyPayment levyPayment)
        {
            Create(levyPayment);
            await Task.CompletedTask;
        }*/

        /*  public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
       PaymentPeriodEnum? period,
       string searchQuery,
       PaginationFilter paginationFilter,
       bool trackChanges = false)
          {
              // Start with base query - using FindByCondition from your repository pattern
              IQueryable<LevyPayment> query = FindByCondition(p => true, trackChanges);

              // Apply period filter if specified
              if (period.HasValue)
              {
                  query = query.Where(l => l.Period == period.Value);
              }

              // Apply search filter if provided
              if (!string.IsNullOrWhiteSpace(searchQuery))
              {
                  searchQuery = searchQuery.Trim().ToLower();
                  bool isNumeric = decimal.TryParse(searchQuery, out decimal searchAmount);

                  query = query.Where(l =>
                      (l.TransactionReference != null && EF.Functions.Like(l.TransactionReference.ToLower(), $"%{searchQuery}%")) ||
                      (isNumeric && l.Amount == searchAmount) ||
                      (l.Notes != null && EF.Functions.Like(l.Notes.ToLower(), $"%{searchQuery}%"))
                  );
              }

              // Apply ordering
              query = query.OrderByDescending(l => l.PaymentDate)
                           .ThenBy(l => l.PaymentStatus);

              // Include related entities - eager loading
              query = query.Include(l => l.Market)
                           .Include(l => l.Trader)
                              .ThenInclude(t => t.User)
                           .Include(l => l.GoodBoy)
                              .ThenInclude(gb => gb.User);

              // Use your existing pagination extension
              return await query.Paginate(paginationFilter);
          }*/

        /*        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
               PaymentPeriodEnum? period,
               string searchQuery,
               PaginationFilter paginationFilter,
               bool trackChanges = false)
                {
                    // Start with base query
                    IQueryable<LevyPayment> query = FindByCondition(p => true, trackChanges);

                    // Apply period filter if specified
                    if (period.HasValue)
                    {
                        query = query.Where(l => l.Period == period.Value);
                    }

                    // Include related entities without conditional logic
                    query = query
                        .Include(l => l.Market)
                        .Include(l => l.Trader)
                            .ThenInclude(t => t.User)
                        .Include(l => l.GoodBoy)
                            .ThenInclude(gb => gb.User);

                    // Apply search filter if provided
                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        searchQuery = searchQuery.Trim().ToLower();
                        bool isNumeric = decimal.TryParse(searchQuery, out decimal searchAmount);

                        query = query.Where(l =>
                            // Search by Trader names
                            (l.Trader != null && l.Trader.User != null && (
                                EF.Functions.Like(l.Trader.User.FirstName.ToLower(), $"%{searchQuery}%") ||
                                EF.Functions.Like(l.Trader.User.LastName.ToLower(), $"%{searchQuery}%"))) ||

                            // Search by GoodBoy names  
                            (l.GoodBoy != null && l.GoodBoy.User != null && (
                                EF.Functions.Like(l.GoodBoy.User.FirstName.ToLower(), $"%{searchQuery}%") ||
                                EF.Functions.Like(l.GoodBoy.User.LastName.ToLower(), $"%{searchQuery}%"))) ||

                            // Search by IDs
                            EF.Functions.Like(l.TraderId ?? "", $"%{searchQuery}%") ||
                            EF.Functions.Like(l.GoodBoyId ?? "", $"%{searchQuery}%") ||

                            // Search by transaction references
                            EF.Functions.Like(l.TransactionReference ?? "", $"%{searchQuery}%") ||

                            // Search by amount if numeric
                            (isNumeric && l.Amount == searchAmount) ||

                            // Search by notes
                            EF.Functions.Like(l.Notes ?? "", $"%{searchQuery}%")
                        );
                    }

                    // Apply ordering
                    query = query
                        .OrderByDescending(l => l.PaymentDate)
                        .ThenBy(l => l.PaymentStatus);

                    return await query.Paginate(paginationFilter);
                }*/
        /* public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPaymentWithDetails(
       PaymentPeriodEnum? period,
       string? searchQuery,
       PaginationFilter paginationFilter,
       bool trackChanges = false)
         {
             IQueryable<LevyPayment> query = FindAll(trackChanges);

             // Apply period filter if specified
             if (period.HasValue)
             {
                 query = query.Where(l => l.Period == period.Value);
             }

             // Apply search filter if provided
             if (!string.IsNullOrWhiteSpace(searchQuery))
             {
                 searchQuery = searchQuery.Trim().ToLower();
                 query = query.Where(l =>
                     // Search by trader name
                     (l.Trader != null && (
                         l.Trader.User.FirstName.ToLower().Contains(searchQuery) ||
                         l.Trader.User.LastName.ToLower().Contains(searchQuery))) ||
                     // Search by GoodBoy name
                     (l.GoodBoy != null && (
                         l.GoodBoy.User.FirstName.ToLower().Contains(searchQuery) ||
                         l.GoodBoy.User.LastName.ToLower().Contains(searchQuery))) ||
                     // Search by amount
                     l.Amount.ToString().Contains(searchQuery) ||
                    // Search by payment status
                    l.PaymentStatus.ToString().ToLower().Contains(searchQuery)
                 );
             }

             // Include related entities needed for display
             query = query
                 .Include(l => l.Market)
                 .Include(l => l.Trader)
                     .ThenInclude(t => t.User)
                 .Include(l => l.GoodBoy)
                     .ThenInclude(gb => gb.User)
                 .OrderByDescending(l => l.PaymentDate)  // Most recent payments first
                 .ThenBy(l => l.PaymentStatus);  // Group by status

             // Apply pagination
             return await query.Paginate(paginationFilter);
         }*/


        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetPagedPayment(int? period, PaginationFilter paginationFilter)
        {
            if (period is not null)
            {
                return await FindAll(false)
                            .Where(l => (int)l.Period == period)
                            .Include(l => l.Market)
                            .Include(l => l.Trader)
                            .Include(l => l.GoodBoy)
                                .ThenInclude(gb => gb.User)
                            .Paginate(paginationFilter);
            }

            return await FindAll(false)
                       .Include(l => l.Market)
                       .Include(l => l.Trader)
                       .Include(l => l.GoodBoy)
                           .ThenInclude(gb => gb.User)
                       .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPaymentsAsync(
            string chairmanId,
            PaginationFilter paginationFilter,
            bool trackChanges)
        {
            return await FindPagedByCondition(
                expression: lp => lp.ChairmanId == chairmanId,
                paginationFilter: paginationFilter,
                trackChanges: trackChanges,
                orderBy: query => query.OrderByDescending(lp => lp.CreatedAt)
            );
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchPayment(
            string searchString,
            PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.TransactionReference.Contains(searchString) ||
                           a.Trader.BusinessName.Contains(searchString) ||
                           a.GoodBoy.User.LastName.Contains(searchString) ||
                           a.GoodBoy.User.FirstName.Contains(searchString))
                           .Include(l => l.Market)
                           .Include(l => l.Trader)
                           .Include(l => l.GoodBoy)
                               .ThenInclude(gb => gb.User)
                           .Paginate(paginationFilter);
        }

        public async Task<decimal> GetTotalLeviesAsync(DateTime startDate, DateTime endDate)
        {
            return await FindByCondition(l =>
                l.PaymentDate >= startDate &&
                l.PaymentDate <= endDate,
                trackChanges: false)
                .SumAsync(l => l.Amount);
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> SearchLevyPaymentsInMarket(
    string marketId,
    string searchQuery,
    PaginationFilter paginationFilter,
    bool trackChanges)
        {
            // Normalize marketId for consistent comparison
            var normalizedMarketId = marketId.ToUpper();

            // Start with base query for the market
            var query = FindByCondition(l => l.MarketId.ToUpper() == normalizedMarketId, trackChanges);

            // Apply search filter if query provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                // Normalize search query
                var normalizedSearch = searchQuery.Trim().ToLower();

                // Check if search query is a number
                decimal amount = 0;
                bool isNumeric = decimal.TryParse(normalizedSearch, out amount);

                // Build the query based on search criteria
                query = query.Where(l =>
                    // Search by trader name (first or last)
                    (l.Trader != null && (
                        l.Trader.User.FirstName.ToLower().Contains(normalizedSearch) ||
                        l.Trader.User.LastName.ToLower().Contains(normalizedSearch) ||
                        (l.Trader.User.FirstName + " " + l.Trader.User.LastName).ToLower().Contains(normalizedSearch) ||
                        (l.Trader.BusinessName != null && l.Trader.BusinessName.ToLower().Contains(normalizedSearch))
                    )) ||
                    // Search by GoodBoy name
                    (l.GoodBoy != null && (
                        l.GoodBoy.User.FirstName.ToLower().Contains(normalizedSearch) ||
                        l.GoodBoy.User.LastName.ToLower().Contains(normalizedSearch) ||
                        (l.GoodBoy.User.FirstName + " " + l.GoodBoy.User.LastName).ToLower().Contains(normalizedSearch)
                    )) ||
                    // Search by transaction reference
                    (l.TransactionReference != null && l.TransactionReference.ToLower().Contains(normalizedSearch))
                );

                // If the search is numeric, add amount search as a separate filter
                // to avoid the out parameter in expression trees
                if (isNumeric)
                {
                    query = query.Where(l => l.Amount == amount || query.Any());
                }
            }

            // Include related entities
            query = query
                .Include(l => l.Trader)
                    .ThenInclude(t => t.User)
                .Include(l => l.GoodBoy)
                    .ThenInclude(g => g.User)
                .OrderByDescending(l => l.PaymentDate);

            // Apply pagination using our extension method
            return await query.Paginate(paginationFilter);
        }
        public void DeleteLevyPayment(LevyPayment levy) => Delete(levy);

        public void DeleteLevySetup(LevySetup levy)
        {
            _context.LevySetups.Remove(levy);
        }

        // Added method to get market-specific levy configuration
        public async Task<LevyPayment> GetMarketLevySetup(string marketId, PaymentPeriodEnum period)
        {
            return await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId && lp.Period == period)
                .OrderByDescending(lp => lp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Set<LevyPayment>()
                .Where(lp => lp.PaymentStatus == PaymentStatusEnum.Paid)
                .SumAsync(lp => lp.Amount);
        }

        public async Task<IEnumerable<LevyPayment>> GetByMarketAndOccupancyAsync(string marketId, MarketTypeEnum traderOccupancy)
        {
            var result = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId && lp.Trader.TraderOccupancy == traderOccupancy)
                .Include(lp => lp.Market)
                .Include(lp => lp.Trader)
                .ToListAsync();
            return result; // Always return the list (empty or populated)
        }

        public async Task<IEnumerable<LevySetup>> GetByMarketAndOccupancies(string marketId, MarketTypeEnum traderOccupancy)
        {
            var result = await _context.LevySetups
                .Where(lp => lp.MarketId == marketId && lp.OccupancyType == traderOccupancy)
                .Include(lp => lp.Market)
                .ToListAsync();
            return result; // Always return the list (empty or populated)
        }

        public async Task<IEnumerable<LevyPayment>> GetByMarketAndOccupancy(string marketId, MarketTypeEnum traderOccupancy)
        {
            var result = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId || lp.Trader.TraderOccupancy == traderOccupancy)
                .Include(lp => lp.Market)
                .Include(lp => lp.Trader)
                .ToListAsync();
            return result; // Always return the list (empty or populated)
        }

        /*public async Task<IEnumerable<LevyPayment>> GetByMarketAndOccupancyAsync(string marketId, MarketTypeEnum traderOccupancy)
        {
            var result = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId && lp.Trader.TraderOccupancy == traderOccupancy)
                .Include(lp => lp.Market)
                .Include(lp => lp.Trader)
                .ToListAsync();

            return result.Any() ? result : null; // Return null if no records are found
        }*/



        public async Task<IQueryable<LevyPayment>> GetMarketLevySetups(string marketId)
        {
            return _context.LevyPayments
                .Where(lp => lp.MarketId == marketId)
                .GroupBy(lp => lp.Period)
                .Select(g => g.OrderByDescending(lp => lp.CreatedAt).First())
                .OrderBy(lp => lp.Period)
                .AsQueryable();
        }

        // Gets the latest levy payment for a trader
        public async Task<LevyPayment> GetLatestLevyPaymentByTraderIdAsync(string traderId)
        {
            return await FindByCondition(
                lp => lp.TraderId == traderId &&
                      lp.PaymentStatus == PaymentStatusEnum.Paid,
                trackChanges: false)
                .OrderByDescending(lp => lp.PaymentDate)
                .ThenByDescending(lp => lp.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // Gets the total amount of levies paid by a trader
        public async Task<decimal> GetTotalLevyAmountByTraderIdAsync(string traderId)
        {
            return await FindByCondition(
                lp => lp.TraderId == traderId &&
                      lp.PaymentStatus == PaymentStatusEnum.Paid,
                trackChanges: false)
                .SumAsync(lp => lp.Amount);
        }

        // Gets the most recent levy payments for a trader
        public async Task<IEnumerable<LevyPayment>> GetRecentLevyPaymentsByTraderIdAsync(
            string traderId,
            int limit = 10)
        {
            return await FindByCondition(
                lp => lp.TraderId == traderId &&
                      lp.PaymentStatus == PaymentStatusEnum.Paid,
                trackChanges: false)
                .OrderByDescending(lp => lp.PaymentDate)
                .ThenByDescending(lp => lp.CreatedAt)
                .Take(limit)
                .Include(lp => lp.Trader)
                .Include(lp => lp.Market)
                .ToListAsync();
        }

        public async Task<LevyPayment> GetLatestActiveLevyForTrader(string traderId)
        {
            return await _context.LevyPayments
                .Where(l => l.TraderId == traderId && l.IsActive)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<LevyPayment>> GetRecentPaymentsForTrader(string traderId, int count)
        {
            return await _context.LevyPayments
                .Where(l => l.TraderId == traderId)
                .OrderByDescending(l => l.PaymentDate)
                .Take(count)
                .ToListAsync();
        }
    }
}

//current
/* public async Task<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>> GetLevyPaymentsByDateRange(
string goodBoyId,
PaginationFilter paginationFilter,
bool trackChanges = false)
 {
     try
     {
         var query = _context.LevyPayments
             .Where(levy => levy.GoodBoyId == goodBoyId &&
                           levy.PaymentStatus == PaymentStatusEnum.Paid &&
                           levy.Period == PaymentPeriodEnum.Daily)
             .GroupJoin(_context.Traders,
                       levy => levy.TraderId,
                       trader => trader.UserId,
                       (levy, traders) => new { levy, traders })
             .SelectMany(x => x.traders.DefaultIfEmpty(),
                        (x, trader) => new { x.levy, trader })
             .Select(x => new GoodBoyLevyPaymentResponseDto
             {
                 Id = x.levy.Id,
                 Amount = x.levy.Amount,
                 PaymentDate = x.levy.PaymentDate,
                 CreatedAt = x.levy.CreatedAt,
                 Status = x.levy.PaymentStatus.ToString(),
                 TraderName = x.trader != null ? x.trader.TraderName : null
             })
             .OrderByDescending(dto => dto.PaymentDate)
             .ThenByDescending(dto => dto.CreatedAt);

         return await query.Paginate(paginationFilter);
     }
     catch (Exception ex)
     {
         Console.WriteLine($"Exception in GetLevyPaymentsByDateRange: {ex.Message}");
         throw;
     }
 }*/
/* public async Task<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>> GetLevyPaymentsByDateRange(
string goodBoyId,
PaginationFilter paginationFilter,
bool trackChanges = false)
 {
     try
     {
         // Build query using joins and project to DTO
         var query = from levy in _context.LevyPayments
                     where levy.GoodBoyId == goodBoyId &&
                           levy.PaymentStatus == PaymentStatusEnum.Paid &&
                           levy.Period == PaymentPeriodEnum.Daily
                     join trader in _context.Traders on levy.TraderId equals trader.Id into traderGroup
                     from trader in traderGroup.DefaultIfEmpty()
                     join user in _context.Users on trader.UserId equals user.Id into userGroup
                     from user in userGroup.DefaultIfEmpty()
                     select new GoodBoyLevyPaymentResponseDto
                     {
                         Id = levy.Id,
                         Amount = levy.Amount,
                         PaymentDate = levy.PaymentDate,
                         CreatedAt = levy.CreatedAt,
                         Status = levy.PaymentStatus.ToString(),
                         TraderName = trader!.TraderName
                     };

         // Apply ordering before pagination
         query = query
             .OrderByDescending(dto => dto.PaymentDate)
             .ThenByDescending(dto => dto.CreatedAt);

         // Use your existing Paginate extension
         return await query.Paginate(paginationFilter);
     }
     catch (Exception ex)
     {
         Console.WriteLine($"Exception in GetLevyPaymentsByDateRange: {ex.Message}");
         throw;
     }
 }*/

/*public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPaymentsByDateRange(
string goodBoyId,
PaginationFilter paginationFilter,
bool trackChanges = false)
{
    try
    {
        // Filter by GoodBoyId, PaymentStatus, and Period=Daily (value 1)
        var query = FindByCondition(
            lp => lp.GoodBoyId == goodBoyId &&
                  lp.PaymentStatus == PaymentStatusEnum.Paid &&
                  lp.Period == PaymentPeriodEnum.Daily,
            trackChanges: trackChanges);

        // Include related entities
        query = query
            .Include(lp => lp.Trader)
                .ThenInclude(t => t.User)
            .Include(lp => lp.Market)
            .OrderByDescending(lp => lp.PaymentDate);

        // Apply pagination using your existing extension method
        return await query.Paginate(paginationFilter);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception in GetLevyPaymentsByDateRange: {ex.Message}");
        throw;
    }
}*/

/* public async Task<IEnumerable<LevyPayment>> GetLevyPaymentsByDateRangeAsync(string goodBoyId, DateTime fromDate, DateTime toDate)
 {
     return await FindByCondition(
         lp => lp.GoodBoyId == goodBoyId &&
               lp.PaymentDate >= fromDate &&
               lp.PaymentDate <= toDate &&  // Changed < to <= to include payments made exactly at toDate
               lp.PaymentStatus == PaymentStatusEnum.Paid,
         trackChanges: false)
         .Include(lp => lp.Trader)
             .ThenInclude(t => t.User)
         .Include(lp => lp.Market)
         .OrderByDescending(lp => lp.PaymentDate)
         .ToListAsync();
 }*/
/* public async Task<IEnumerable<LevyPayment>> GetLevyPaymentsByDateRange(string goodBoyId, DateTime fromDate, DateTime toDate)
 {
     return await FindByCondition(
         lp => lp.GoodBoyId == goodBoyId &&
               lp.PaymentDate >= fromDate &&
               lp.PaymentDate < toDate &&
               lp.PaymentStatus == PaymentStatusEnum.Paid,
         trackChanges: false)
         .Include(lp => lp.Trader)
             .ThenInclude(t => t.User)
         .Include(lp => lp.Market)
         .OrderByDescending(lp => lp.PaymentDate)
         .ToListAsync();
 }*/

/*public async Task<IEnumerable<LevyPayment>> GetLevyPaymentsByDateRange(string goodBoyId)
{
    // Get the current month in YYYYMM format
    string currentMonth = DateTime.Now.ToString("yyyyMM");

    // Debug info
    Console.WriteLine($"Searching for goodBoyId: {goodBoyId}, month: {currentMonth}");

    return await FindByCondition(
        lp => lp.GoodBoyId == goodBoyId &&
              // Try using string comparison for the period if stored as YYYYMM
              lp.Period.ToString() == currentMonth &&
              // If PaymentStatus of 2 means "Paid" in your system
              (int)lp.PaymentStatus == 2,
        trackChanges: false)
        .Include(lp => lp.Trader)
            .ThenInclude(t => t.User)
        .Include(lp => lp.Market)
        .OrderByDescending(lp => lp.PaymentDate) // or other sort column
        .ToListAsync();
}*/

/* public async Task<IEnumerable<LevyPayment>> GetLevyPaymentsByDateRange(string goodBoyId)
 {
     try
     {
         // Filter by GoodBoyId, PaymentStatus, and Period=Daily (value 1)
         var query = FindByCondition(
             lp => lp.GoodBoyId == goodBoyId &&
                   lp.PaymentStatus == PaymentStatusEnum.Paid &&
                   lp.Period == PaymentPeriodEnum.Daily,
             trackChanges: false);

         // Include related entities
         query = query
             .Include(lp => lp.Trader)
                 .ThenInclude(t => t.User)
             .Include(lp => lp.Market)
             .OrderByDescending(lp => lp.PaymentDate);

         return await query.ToListAsync();
     }
     catch (Exception ex)
     {
         Console.WriteLine($"Exception in GetLevyPaymentsByDateRange: {ex.Message}");
         throw;
     }
 }*/