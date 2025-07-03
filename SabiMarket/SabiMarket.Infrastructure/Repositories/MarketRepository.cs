using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{

    public class MarketRepository : GeneralRepository<Market>, IMarketRepository
    {
        private readonly ApplicationDbContext _context;
        public MarketRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddMarket(Market market) => Create(market);

        public void DeleteMarket(Market market) => Delete(market);
        /*     public async Task<IEnumerable<Market>> GetAllMarketForExport(bool trackChanges, string searchQuery = null)
             {
                 var query = FindAll(trackChanges)
                     .Include(a => a.Caretaker)
                         .ThenInclude(c => c.User)
                     .Include(a => a.Traders)
                     .Include(m => m.Chairman)
                         .ThenInclude(c => c.User)
                     .Include(a => a.LocalGovernment)
                     .Include(a => a.MarketSections);

                 // If implementing search at repository level for better performance
                 if (!string.IsNullOrEmpty(searchQuery))
                 {
                     searchQuery = searchQuery.ToLower();
                     query = query.Where(m =>
                         (m.MarketName != null && m.MarketName.ToLower().Contains(searchQuery)) ||
                         (m.Location != null && m.Location.ToLower().Contains(searchQuery)) ||
                         (m.Description != null && m.Description.ToLower().Contains(searchQuery)) ||
                         (m.LocalGovernment != null && m.LocalGovernment.Name != null && m.LocalGovernment.Name.ToLower().Contains(searchQuery)) ||
                         (m.LocalGovernmentName != null && m.LocalGovernmentName.ToLower().Contains(searchQuery)) ||
                         (m.Chairman != null && m.Chairman.User != null && (
                             (m.Chairman.User.FirstName != null && m.Chairman.User.FirstName.ToLower().Contains(searchQuery)) ||
                             (m.Chairman.User.LastName != null && m.Chairman.User.LastName.ToLower().Contains(searchQuery))
                         )) ||
                         (m.Caretaker != null && m.Caretaker.User != null && (
                             (m.Caretaker.User.FirstName != null && m.Caretaker.User.FirstName.ToLower().Contains(searchQuery)) ||
                             (m.Caretaker.User.LastName != null && m.Caretaker.User.LastName.ToLower().Contains(searchQuery))
                         )) ||
                         (m.MarketType != null && m.MarketType.ToLower().Contains(searchQuery))
                     );
                 }

                 return await query.ToListAsync();
             }*/

        public IQueryable<Market> GetMarketsByCaretakerId(string caretakerId)
        {
            return FindByCondition(m => m.CaretakerId == caretakerId, false)
                .Include(m => m.Caretaker)
                .Include(m => m.LocalGovernment);
        }
        public async Task<IEnumerable<Market>> GetAllMarketForExport(bool trackChanges, string searchQuery = null)
        {
            // Start with base query
            var query = FindAll(trackChanges);

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(m =>
                    (m.MarketName != null && m.MarketName.ToLower().Contains(searchQuery)) ||
                    (m.Location != null && m.Location.ToLower().Contains(searchQuery)) ||
                    (m.Description != null && m.Description.ToLower().Contains(searchQuery)) ||
                    (m.LocalGovernmentName != null && m.LocalGovernmentName.ToLower().Contains(searchQuery)) ||
                    (m.MarketType != null && m.MarketType.ToLower().Contains(searchQuery))
                );
            }

            // Apply includes after filtering
            var queryWithIncludes = query
                .Include(a => a.Caretaker)
                    .ThenInclude(c => c.User)
                .Include(a => a.Traders)
                .Include(m => m.Chairman)
                    .ThenInclude(c => c.User)
                .Include(a => a.LocalGovernment)
                .Include(a => a.MarketSections);

            // Get the markets with includes
            var markets = await queryWithIncludes.ToListAsync();

            // If we need to search on navigation properties, do it in memory after loading
            if (!string.IsNullOrEmpty(searchQuery))
            {
                markets = markets.Where(m =>
                    (m.MarketName != null && m.MarketName.ToLower().Contains(searchQuery)) ||
                    (m.Location != null && m.Location.ToLower().Contains(searchQuery)) ||
                    (m.Description != null && m.Description.ToLower().Contains(searchQuery)) ||
                    (m.LocalGovernment != null && m.LocalGovernment.Name != null && m.LocalGovernment.Name.ToLower().Contains(searchQuery)) ||
                    (m.LocalGovernmentName != null && m.LocalGovernmentName.ToLower().Contains(searchQuery)) ||
                    (m.Chairman != null && m.Chairman.User != null && (
                        (m.Chairman.User.FirstName != null && m.Chairman.User.FirstName.ToLower().Contains(searchQuery)) ||
                        (m.Chairman.User.LastName != null && m.Chairman.User.LastName.ToLower().Contains(searchQuery))
                    )) ||
                    (m.Caretaker != null && m.Caretaker.User != null && (
                        (m.Caretaker.User.FirstName != null && m.Caretaker.User.FirstName.ToLower().Contains(searchQuery)) ||
                        (m.Caretaker.User.LastName != null && m.Caretaker.User.LastName.ToLower().Contains(searchQuery))
                    )) ||
                    (m.MarketType != null && m.MarketType.ToLower().Contains(searchQuery))
                ).ToList();
            }

            return markets;
        }


        public async Task<Market> GetMarketById(string id, bool trackChanges) => await FindByCondition(x => x.Id == id, trackChanges)
            .Include(a => a.Caretaker)
            .Include(a => a.Traders)
            .Include(a => a.MarketSections)
            .Include(a => a.LocalGovernment)
            .FirstOrDefaultAsync();

        public async Task<Market> GetMarketByUserId(string userId, bool trackChanges) => await FindByCondition(x => x.Id == userId, trackChanges)
           .Include(a => a.Caretaker)
           .Include(a => a.Traders)
           .Include(a => a.MarketSections)
           .Include(a => a.LocalGovernment)
           .FirstOrDefaultAsync();

        public IQueryable<Market> GetMarketsQuery()
        {
            return FindAll(trackChanges: false)
                .Include(m => m.Chairman)
                .Include(m => m.Caretaker)
                .Include(m => m.Traders)
                .Include(m => m.LocalGovernment);
        }
        public async Task<PaginatorDto<IEnumerable<Market>>> GetPagedMarket(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                        .Include(a => a.Caretaker)
                        .Include(a => a.Traders)
                        .Include(a => a.MarketSections)
                        .Include(a => a.LocalGovernment)
                        .Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<Market>>> SearchMarket(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                           .Where(a => a.LocalGovernment.Name.Contains(searchString) ||
                           a.MarketName.Contains(searchString))
                           .Paginate(paginationFilter);
        }

        public async Task<Market> GetMarketByIdAsync(string marketId, bool trackChanges)
        {
            // Fetch the market with its relationships
            var market = await FindByCondition(m => m.Id == marketId, trackChanges)
                .Include(m => m.Caretaker)
                    .ThenInclude(c => c.User)
                .Include(m => m.Traders)
                    .ThenInclude(t => t.User)
                .Include(m => m.Chairman)
                .Include(m => m.LocalGovernment)
                .Include(m => m.MarketSections)
                .FirstOrDefaultAsync();

            if (market != null)
            {
                // Initialize AdditionalCaretakers if null
                if (market.AdditionalCaretakers == null)
                    market.AdditionalCaretakers = new List<Caretaker>();

                // Load all caretakers for this market
                var allCaretakers = await _context.Caretakers
                    .Include(c => c.User)
                    .Where(c => c.MarketId == marketId)
                    .AsNoTracking()
                    .ToListAsync();

                // Filter out the primary caretaker
                var additionalCaretakers = market.CaretakerId != null
                    ? allCaretakers.Where(c => c.Id != market.CaretakerId).ToList()
                    : allCaretakers;

                // Add to collection
                foreach (var caretaker in additionalCaretakers)
                {
                    market.AdditionalCaretakers.Add(caretaker);
                }
            }

            return market;
        }

        /*   public async Task<Market> GetMarketByIdAsync(string marketId, bool trackChanges)
           {
               // Fetch the market with its primary caretaker
               var market = await FindByCondition(m => m.Id == marketId, trackChanges)
                   .Include(m => m.Caretaker)
                       .ThenInclude(c => c.User)
                   .Include(m => m.Traders)
                       .ThenInclude(t => t.User)
                   .Include(m => m.Chairman)
                   .Include(m => m.LocalGovernment)
                   .Include(m => m.MarketSections)
                   .Include(m => m.AdditionalCaretakers) // Include additional caretakers directly
                       .ThenInclude(c => c.User)
                   .FirstOrDefaultAsync();

               if (market != null)
               {
                   var additionalCaretakers = await _context.Caretakers
                            .Include(c => c.User)
                            .Where(c => c.MarketId == marketId && (market.CaretakerId == null || c.Id != market.CaretakerId))
                            .AsNoTracking()  // For performance
                            .ToListAsync();

                   // Create a collection for all caretakers (if not exists)
                   if (market.AdditionalCaretakers == null)
                   {
                       market.AdditionalCaretakers = new List<Caretaker>();
                   }

                   // Add additional caretakers
                   foreach (var caretaker in additionalCaretakers)
                   {
                       market.AdditionalCaretakers.Add(caretaker);
                   }
               }

               return market;
           }*/
        /*   public async Task<Market> GetMarketByIdAsync(string marketId, bool trackChanges)
           {
               var query = FindByCondition(m => m.Id == marketId, trackChanges);

               query = query
                   .Include(a => a.Caretaker)
                   .Include(a => a.Traders)
                   .Include(a => a.MarketSections)
                   .Include(a => a.LocalGovernment);

               return await query.FirstOrDefaultAsync();
           }*/

        public async Task<Market> GetMarketRevenueAsync(string marketId, DateTime startDate, DateTime endDate)
        {
            var market = await FindByCondition(m => m.Id == marketId, trackChanges: false)
                .Include(m => m.Traders)
                .Include(m => m.LocalGovernment)
                .Include(m => m.MarketSections)
                .FirstOrDefaultAsync();

            if (market == null)
                return null;

            // Get levy payments for this market's traders within the date range
            var levyPayments = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId &&
                       lp.PaymentDate >= startDate &&
                       lp.PaymentDate <= endDate)
                .ToListAsync();

            // Create the revenue entity
            var marketRevenue = new Market
            {
                Id = market.Id,
                MarketName = market.MarketName,
                TotalRevenue = levyPayments.Sum(lp => lp.Amount),
                PaymentTransactions = levyPayments.Count,
                Location = market.Location,
                LocalGovernmentName = market.LocalGovernment?.Name,
                StartDate = startDate,
                EndDate = endDate,
                TotalTraders = market.Traders?.Count ?? 0,
                MarketCapacity = market.Capacity,
                OccupancyRate = market.Capacity > 0
                    ? (decimal)(market.Traders?.Count ?? 0) / market.Capacity * 100
                    : 0
            };

            return marketRevenue;

        }


        public async Task<Market> GetComplianceRatesAsync(string marketId)
        {
            var market = await FindByCondition(m => m.Id == marketId, trackChanges: false)
                .Include(m => m.Traders)
                .Include(m => m.LocalGovernment)
                .Include(m => m.MarketSections)
                .FirstOrDefaultAsync();

            if (market == null)
                return null;

            // Get all levy payments for this market
            var levyPayments = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId)
                .ToListAsync();

            // Calculate compliance metrics
            var totalTraders = market.Traders?.Count ?? 0;
            var tradersWithPayments = levyPayments
                .Select(lp => lp.TraderId)
                .Distinct()
                .Count();

            // Create the compliance entity
            var marketCompliance = new Market
            {
                Id = market.Id,
                MarketName = market.MarketName,
                Location = market.Location,
                LocalGovernmentName = market.LocalGovernment?.Name,
                TotalTraders = totalTraders,
                ComplianceRate = totalTraders > 0
                    ? (decimal)tradersWithPayments / totalTraders * 100
                    : 0,
                CompliantTraders = tradersWithPayments,
                NonCompliantTraders = totalTraders - tradersWithPayments
            };

            return marketCompliance;
        }

    }
}


/*  public async Task<Market> GetMarketByIdAsync(string marketId, bool trackChanges)
        {
            // Fetch the market with its relationships
            var market = await FindByCondition(m => m.Id == marketId, trackChanges)
                .Include(m => m.Caretaker)
                    .ThenInclude(c => c.User)
                .Include(m => m.Traders)
                    .ThenInclude(t => t.User)
                .Include(m => m.Chairman)
                .Include(m => m.LocalGovernment)
                .Include(m => m.MarketSections)
                .FirstOrDefaultAsync();

           *//* if (market != null)
            {
                // Debugging: Check if traders have their user data loaded
                if (market.Traders != null)
                {
                    foreach (var trader in market.Traders)
                    {
                        // Log or check if User is properly loaded
                        Console.WriteLine($"Trader ID: {trader.Id}, User loaded: {trader.User != null}");

                        if (trader.User != null)
                        {
                            Console.WriteLine($"User details: {trader.User.FirstName} {trader.User.LastName}, Email: {trader.User.Email}");
                        }
                        else
                        {
                            Console.WriteLine($"User is null for trader {trader.Id} with UserId {trader.UserId}");

                            // Try to explicitly load the User if it's null
                            if (!string.IsNullOrEmpty(trader.UserId))
                            {
                                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == trader.UserId);
                                if (user != null)
                                {
                                    trader.User = user;
                                    Console.WriteLine($"Explicitly loaded user: {user.FirstName} {user.LastName}");
                                }
                                else
                                {
                                    Console.WriteLine($"Could not find user with ID {trader.UserId}");
                                }
                            }
                        }
                    }
                }*//*

                // Rest of your existing code for caretakers...
                // Initialize AdditionalCaretakers if null
                if (market.AdditionalCaretakers == null)
                    market.AdditionalCaretakers = new List<Caretaker>();

                // Load all caretakers for this market
                var allCaretakers = await _context.Caretakers
                    .Include(c => c.User)
                    .Where(c => c.MarketId == marketId)
                    .AsNoTracking()
                    .ToListAsync();

                // Filter out the primary caretaker
                var additionalCaretakers = market.CaretakerId != null
                    ? allCaretakers.Where(c => c.Id != market.CaretakerId).ToList()
                    : allCaretakers;

                // Add to collection
                foreach (var caretaker in additionalCaretakers)
                {
                    market.AdditionalCaretakers.Add(caretaker);
                }
            }

            return market;
        }*/