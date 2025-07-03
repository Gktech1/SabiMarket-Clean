using iText.Commons.Actions.Contexts;
using Mailjet.Client.Resources;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class CaretakerRepository : GeneralRepository<Caretaker>, ICaretakerRepository
    {
        private readonly ApplicationDbContext _repositoryContext;
        public CaretakerRepository(ApplicationDbContext repositoryContext)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public IQueryable<Caretaker> GetCaretakersQuery()
        {
            return FindAll(trackChanges: false)
                .Include(c => c.User)
                .Include(c => c.Chairman)
                .Include(c => c.Markets);
        }
        public async Task<Caretaker> GetCaretakerById(string userId, bool trackChanges) =>
         await FindByCondition(x => x.UserId == userId, trackChanges)
             .Include(a => a.User)  // Include the User entity
             .Include(a => a.Markets)
             .Include(a => a.GoodBoys)
                 .ThenInclude(gb => gb.LevyPayments)
             .Include(a => a.AssignedTraders)
                 .ThenInclude(t => t.LevyPayments)
             .FirstOrDefaultAsync();

        public async Task<Caretaker> GetCaretakerByMarketId(string marketId, bool trackChanges) =>
            await FindByCondition(x => x.MarketId == marketId, trackChanges)
                .Include(a => a.Markets)
                .Include(a => a.GoodBoys)
                    .ThenInclude(gb => gb.LevyPayments)
                .Include(a => a.AssignedTraders)
                    .ThenInclude(t => t.Market)
                .FirstOrDefaultAsync();

        public async Task<Caretaker> GetCaretakerByLocalGovernmentId(string LGAId, bool trackChanges) =>
           await FindByCondition(x => x.LocalGovernmentId == LGAId, trackChanges)
               .Include(a => a.Markets)
               .Include(a => a.GoodBoys)
                   .ThenInclude(gb => gb.LevyPayments)
               .Include(a => a.AssignedTraders)
                   .ThenInclude(t => t.Market)
               .FirstOrDefaultAsync();

        /* public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(
             PaginationFilter paginationFilter, bool trackChanges)
         {
             var query = FindAll(trackChanges)
                 .Include(a => a.Markets)
                 .Include(a => a.GoodBoys)
                 .Include(a => a.AssignedTraders)
                 .OrderBy(c => c.CreatedAt);

             return await query.Paginate(paginationFilter);
         }*/

        /*  public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(
      PaginationFilter paginationFilter, bool trackChanges)
          {
              var query = FindAll(trackChanges)
                  .Include(a => a.Markets)
                  .Include(a => a.GoodBoys)
                  .Include(a => a.AssignedTraders)
                  .Include(a => a.User) // Include User to populate firstName, lastName, email, etc.
                  .OrderBy(c => c.CreatedAt);

              return await query.Paginate(paginationFilter);
          }*/

        /*    public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(
                PaginationFilter paginationFilter, bool trackChanges)
            {
                try
                {
                    // Explicitly select query to ensure user properties are included
                    var query = FindAll(trackChanges)
                        .Include(a => a.User) // Include User to populate firstName, lastName, email, etc.
                        .Include(a => a.Markets)
                        .Include(a => a.GoodBoys)
                        .Include(a => a.AssignedTraders)
                        .OrderBy(c => c.CreatedAt);

                     Console.WriteLine("Executing caretaker query with Include(User)");

                    // Debug: Check if User properties are loaded
                    var testCaretakers = await query.Take(2).ToListAsync();
                    foreach (var c in testCaretakers)
                    {
                        Console.WriteLine($"Caretaker {c.Id} - User null? {c.User == null}");
                        if (c.User != null)
                        {
                            Console.WriteLine($"   User {c.UserId} - FirstName: '{c.User.FirstName}', LastName: '{c.User.LastName}'");
                        }
                    }

                    return await query.Paginate(paginationFilter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving caretakers with pagination {ex}");
                    throw;
                }
            }*/

        public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersWithPagination(
    PaginationFilter paginationFilter, bool trackChanges)
        {
            // Use a more direct approach with manual joining
            var query = from caretaker in _repositoryContext.Set<Caretaker>()
                        join user in _repositoryContext.Set<ApplicationUser>() on caretaker.UserId equals user.Id
                        select new
                        {
                            Caretaker = caretaker,
                            User = user
                        };

            // Execute query and populate navigation properties manually
            var results = await query
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToListAsync();

            // Count total for pagination
            var totalCount = await query.CountAsync();

            // Manually assign User to each Caretaker
            var caretakers = results.Select(r => {
                r.Caretaker.User = r.User; // Attach User entity
                return r.Caretaker;
            }).ToList();

            // Create paginator result
            return new PaginatorDto<IEnumerable<Caretaker>>
            {
                PageItems = caretakers,
                CurrentPage = paginationFilter.PageNumber,
                PageSize = paginationFilter.PageSize,
                NumberOfPages = (totalCount + paginationFilter.PageSize - 1) / paginationFilter.PageSize
            };
        }

        public async Task<PaginatorDto<IEnumerable<Caretaker>>> GetCaretakersAsync(
           string chairmanId, PaginationFilter paginationFilter, bool trackChanges)
        {
            return await FindPagedByCondition(
                expression: c => c.ChairmanId == chairmanId,
                paginationFilter: paginationFilter,
                trackChanges: trackChanges,
                orderBy: query => query.OrderBy(c => c.CreatedAt));
        }

        public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPayments(
     string caretakerId, PaginationFilter paginationFilter, bool trackChanges)
        {
            // Get GoodBoys and Traders for this caretaker
            var goodBoyIds = await _repositoryContext.GoodBoys
                .Where(gb => gb.CaretakerId == caretakerId)
                .Select(gb => gb.Id)
                .ToListAsync();

            var traderIds = await _repositoryContext.Traders
                .Where(t => t.CaretakerId == caretakerId)
                .Select(t => t.Id)
                .ToListAsync();

            // Get LevyPayments for these GoodBoys and Traders
            var query = _repositoryContext.LevyPayments
                .Include(lp => lp.GoodBoy)
                .Include(lp => lp.Trader)
                .Include(lp => lp.Chairman)
                .Include(lp => lp.Market)
                .Where(lp => goodBoyIds.Contains(lp.GoodBoyId) || traderIds.Contains(lp.TraderId))
                .OrderByDescending(lp => lp.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        /* public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPayments(
     string caretakerId, PaginationFilter paginationFilter, bool trackChanges)
         {
             // Let's test step by step
             try
             {
                 // 1. First check if LevyPayments exist
                 var allLevyPayments = await _repositoryContext.LevyPayments.ToListAsync();
                 Console.WriteLine($"Total LevyPayments: {allLevyPayments.Count}");

                 // 2. Check if we can load with includes
                 var levyPaymentsWithIncludes = await _repositoryContext.LevyPayments
                     .Include(lp => lp.GoodBoy)
                     .Include(lp => lp.Trader)
                     .ToListAsync();
                 Console.WriteLine($"LevyPayments with includes: {levyPaymentsWithIncludes.Count}");

                 // 3. Check navigation properties
                 foreach (var lp in levyPaymentsWithIncludes)
                 {
                     Console.WriteLine($"LevyPayment ID: {lp.Id}");
                     Console.WriteLine($"GoodBoy: {lp.GoodBoy?.Id}, CaretakerId: {lp.GoodBoy?.CaretakerId}");
                     Console.WriteLine($"Trader: {lp.Trader?.Id}, CaretakerId: {lp.Trader?.CaretakerId}");
                 }

                 // 4. Now try the actual query
                 var query = _repositoryContext.LevyPayments
                     .Include(lp => lp.GoodBoy)
                     .Include(lp => lp.Trader)
                     .Where(lp => (lp.GoodBoy != null && lp.GoodBoy.CaretakerId == caretakerId) ||
                                  (lp.Trader != null && lp.Trader.CaretakerId == caretakerId))
                     .OrderByDescending(lp => lp.CreatedAt);

                 var result = await query.ToListAsync();
                 Console.WriteLine($"Filtered result count: {result.Count}");

                 return await query.Paginate(paginationFilter);
             }
             catch (Exception ex)
             {
               //  _logger.LogError(ex, "Error in GetLevyPayments");
                 throw;
             }
         }
 */
        /*  public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPayments(
      string caretakerId, PaginationFilter paginationFilter, bool trackChanges)
          {
              // First, let's check if we can get all levy payments
              var allPayments = await _repositoryContext.LevyPayments
                  .Include(lp => lp.GoodBoy)
                  .Include(lp => lp.Trader)
                  .ToListAsync();

              Console.WriteLine($"Total payments: {allPayments.Count}");

              // Now filter in memory to see what's happening
              var filteredPayments = allPayments
                  .Where(lp => (lp.GoodBoy != null && lp.GoodBoy.CaretakerId == caretakerId) ||
                               (lp.Trader != null && lp.Trader.CaretakerId == caretakerId))
                  .ToList();

              Console.WriteLine($"Filtered payments: {filteredPayments.Count}");

              // Now do the actual query
              var query = _repositoryContext.LevyPayments
                  .Include(lp => lp.GoodBoy)
                  .Include(lp => lp.Trader)
                  .Where(lp => (lp.GoodBoy != null && lp.GoodBoy.CaretakerId == caretakerId) ||
                               (lp.Trader != null && lp.Trader.CaretakerId == caretakerId))
                  .OrderByDescending(lp => lp.CreatedAt);

              return await query.Paginate(paginationFilter);
          }
  */
        /*public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPayments(
    string caretakerId, PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = _repositoryContext.LevyPayments
                .Include(lp => lp.GoodBoy)
                .Include(lp => lp.Trader)
                .Where(lp => (lp.GoodBoy != null && lp.GoodBoy.CaretakerId == caretakerId) ||
                             (lp.Trader != null && lp.Trader.CaretakerId == caretakerId))
                .OrderByDescending(lp => lp.CreatedAt);

            return await query.Paginate(paginationFilter);
        }*/
        /*public async Task<PaginatorDto<IEnumerable<LevyPayment>>> GetLevyPayments(
            string caretakerId, PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = _repositoryContext.LevyPayments
                .Where(lp => lp.GoodBoy.CaretakerId == caretakerId ||
                            lp.Trader.CaretakerId == caretakerId)
                .Include(lp => lp.GoodBoy)
                .Include(lp => lp.Trader)
                .OrderByDescending(lp => lp.CreatedAt);

            return await query.Paginate(paginationFilter);
        }*/

        public async Task<LevyPayment> GetLevyPaymentDetails(string levyId, bool trackChanges) =>
            await _repositoryContext.LevyPayments
                .Where(lp => lp.Id == levyId)
                .Include(lp => lp.GoodBoy)
                .Include(lp => lp.Trader)
                .FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<GoodBoy>>> GetGoodBoys(
            string caretakerId, PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = _repositoryContext.GoodBoys
                .Where(gb => gb.CaretakerId == caretakerId)
                .Include(gb => gb.User)
                .Include(gb => gb.LevyPayments)
                .OrderBy(gb => gb.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        public async Task<bool> CaretakerExists(string chairmanId, string marketId) =>
            await FindByCondition(x => x.UserId == chairmanId && x.MarketId == marketId,
                trackChanges: false).AnyAsync();

        public void CreateCaretaker(Caretaker caretaker) => Create(caretaker);

        public void UpdateCaretaker(Caretaker updatecaretaker) => Update(updatecaretaker);

        public void DeleteCaretaker(Caretaker caretaker) => Delete(caretaker);

        /*public async Task<int> GetCaretakerCountAsync()
        {
            return await FindAll(trackChanges: false).CountAsync();
        }*/

        public async Task<int> GetCaretakerCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
                var query = FindAll(trackChanges: false);

                if (startDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= endDate.Value);
                }

                // Optional: You might want to only count active caretakers
                query = query.Where(c => !c.IsBlocked);

                return await query.CountAsync();
        }

        public async Task<IEnumerable<Caretaker>> GetAllCaretakers(bool trackChanges) =>
            await FindAll(trackChanges)
                .Include(u => u.User)
                .Include(c => c.Markets)
                .Include(c => c.GoodBoys)
                .Include(c => c.AssignedTraders)
                .ToListAsync();

        public async Task<IEnumerable<Caretaker>> GetAllCaretakersByUserId( string userId, bool trackChanges) =>
        await FindAll(trackChanges)
            .Where(u => u.UserId == userId)
                .Include(u => u.User)
                .Include(c => c.Markets)
                .Include(c => c.GoodBoys)
                .Include(c => c.AssignedTraders)
                .ToListAsync();

        public async Task<bool> CaretakerExistsAsync(string useriId)
        {
            if (string.IsNullOrEmpty(useriId))
                return false;

            return await _repositoryContext.Caretakers
                .AnyAsync(c => c.UserId == useriId);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            return await _repositoryContext.Caretakers
                .AnyAsync(c => c.Id == id);
        }
    }
}

