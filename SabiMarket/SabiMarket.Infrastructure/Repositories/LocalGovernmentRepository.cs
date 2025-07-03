using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using System.Linq;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace SabiMarket.Infrastructure.Repositories
{
    public class LocalGovernmentRepository : GeneralRepository<LocalGovernment>, ILocalGovernmentRepository
    {
        private readonly ApplicationDbContext _repositoryContext;

        public LocalGovernmentRepository(ApplicationDbContext repositoryContext)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public async Task<LocalGovernment> GetLocalGovernmentById(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .FirstOrDefaultAsync();

        public async Task<LocalGovernment> GetLocalGovernmentWithUsers(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .Include(lg => lg.Users)
                .FirstOrDefaultAsync();

        public async Task<LocalGovernment> GetLocalGovernmentWithMarkets(string id, bool trackChanges) =>
            await FindByCondition(lg => lg.Id == id, trackChanges)
                .Include(lg => lg.Markets)
                .FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentsWithPagination(
            PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = FindAll(trackChanges)
                .Include(lg => lg.Markets)
                .OrderBy(lg => lg.Name);

            return await query.Paginate(paginationFilter);
        }

        public async Task<LocalGovernment> GetLocalGovernmentByName(
            string name, string state, bool trackChanges) =>
            await FindByCondition(
                lg => lg.Name.ToLower() == name.ToLower() &&
                      lg.State.ToLower() == state.ToLower(),
                trackChanges)
            .FirstOrDefaultAsync();

        public async Task<decimal> GetTotalRevenue(string localGovernmentId)
        {
            var localGovernment = await FindByCondition(
                lg => lg.Id == localGovernmentId, false)
                .FirstOrDefaultAsync();

            return localGovernment?.CurrentRevenue ?? 0;
        }

        public async Task<bool> LocalGovernmentExists(string name, string state) =>
            await FindByCondition(
                lg => lg.Name.ToLower() == name.ToLower() &&
                      lg.State.ToLower() == state.ToLower(),
                trackChanges: false)
            .AnyAsync();

        public async Task<bool> LocalGovernmentExist(string localgovernmentId) =>
            await FindByCondition(
                lg => lg.Id == localgovernmentId,
                trackChanges: false)
            .AnyAsync();
        public void CreateLocalGovernment(LocalGovernment localGovernment) =>
            Create(localGovernment);

        public void UpdateLocalGovernment(LocalGovernment localGovernment) =>
            Update(localGovernment);

        public void DeleteLocalGovernment(LocalGovernment localGovernment) =>
            Delete(localGovernment);

        public async Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentArea(
      string searchTerm,
      PaginationFilter paginationFilter)
        {
            // Start with Chairman query and include LocalGovernment
            var query = _repositoryContext.Chairmen
                .AsNoTracking()
                .Include(c => c.LocalGovernment)
                .Select(chairman => new LocalGovernment
                {
                    Id = chairman.LocalGovernment.Id,
                    Name = chairman.LocalGovernment.Name,
                    LGA = chairman.FullName ?? "Not Assigned"
                });

            // For LGAs without chairmen, union with LGAs that don't have chairmen
            var lgasWithoutChairmen = _repositoryContext.LocalGovernments
                .AsNoTracking()
                .Where(lg => !_repositoryContext.Chairmen.Any(c => c.LocalGovernmentId == lg.Id))
                .Select(lg => new LocalGovernment
                {
                    Id = lg.Id,
                    Name = lg.Name,
                    LGA = "Not Assigned"
                });

            // Combine both queries
            query = query.Union(lgasWithoutChairmen);

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(lg =>
                    lg.Name.ToLower().Contains(term) ||
                    lg.LGA.ToLower().Contains(term));
            }

            // Order by name
            query = query.OrderBy(lg => lg.Name);

            // Use the Paginate extension method
            return await query.Paginate(paginationFilter);
        }

        public async Task<int> CountAsync()
        {
            return await _repositoryContext.Set<LocalGovernment>().CountAsync();
        }

        /* public async Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentArea(
       string searchTerm,
       PaginationFilter paginationFilter)
   {
       // Start with base query and include Chairman relationship
       var query = _repositoryContext.LocalGovernments
           .AsNoTracking()
           .GroupJoin(
               _repositoryContext.Chairmen,
               lg => lg.Id,
               c => c.LocalGovernmentId,
               (lg, chairmen) => new { LocalGovt = lg, Chairmen = chairmen })
           .SelectMany(
               x => x.Chairmen.DefaultIfEmpty(),
               (lg, chairman) => new LocalGovernment
               {
                   Id = lg.LocalGovt.Id,
                   Name = lg.LocalGovt.Name,
                   LGA = chairman != null ? chairman.FullName : "Not Assigned"  // Store chairman name in LGA property
               });

       // Apply search term filter
       if (!string.IsNullOrWhiteSpace(searchTerm))
       {
           var term = searchTerm.ToLower();
           query = query.Where(lg =>
               lg.Name.ToLower().Contains(term) ||
               lg.LGA.ToLower().Contains(term));  // Search in both name and chairman name
       }

       // Order by name
       query = query.OrderBy(lg => lg.Name);

       // Use the Paginate extension method
       return await query.Paginate(paginationFilter);
   }*/
        /*    public async Task<PaginatorDto<IEnumerable<LocalGovernment>>> GetLocalGovernmentArea(
          string searchTerm,
          PaginationFilter paginationFilter)
            {
                // Start with base query and include related entities
                var query = FindAll(trackChanges: false)
                    .Include(lg => lg.AssistCenterOfficers)
                        .ThenInclude(aco => aco.User)
                    .AsNoTracking();

                // Apply search term filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.ToLower();
                    query = query.Where(lg =>
                        lg.Name.ToLower().Contains(term) ||
                        lg.Address.ToLower().Contains(term) ||
                        (lg.LGA != null && lg.LGA.ToLower().Contains(term)));
                }

                // Apply pagination and return results
                return await query.Paginate(paginationFilter);
            }
    */
        private static IQueryable<LocalGovernment> ApplyLocalGovernmentOrdering(
            IQueryable<LocalGovernment> query,
            string orderBy,
            bool isDescending)
        {
            return (orderBy?.ToLower(), isDescending) switch
            {
                ("name", true) => query.OrderByDescending(lg => lg.Name),
                ("name", false) => query.OrderBy(lg => lg.Name),
                ("state", true) => query.OrderByDescending(lg => lg.State),
                ("state", false) => query.OrderBy(lg => lg.State),
                ("revenue", true) => query.OrderByDescending(lg => lg.CurrentRevenue),
                ("revenue", false) => query.OrderBy(lg => lg.CurrentRevenue),
                ("createdat", true) => query.OrderByDescending(lg => lg.CreatedAt),
                ("createdat", false) => query.OrderBy(lg => lg.CreatedAt),
                ("lga", true) => query.OrderByDescending(lg => lg.LGA),
                ("lga", false) => query.OrderBy(lg => lg.LGA),
                _ => query.OrderBy(lg => lg.Name) // Default ordering
            };
        }

  
        public IQueryable<LocalGovernment> GetFilteredLGAsQuery(LGAFilterRequestDto filterDto)
        {
            // Make sure to use AsNoTracking() for read-only operations
            var query = FindAll(trackChanges: false).AsNoTracking();

            // Apply filters only if they exist (using case-insensitive comparison)
            if (!string.IsNullOrWhiteSpace(filterDto.State))
            {
                var state = filterDto.State.ToLower();
                query = query.Where(lg => lg.State.ToLower() == state);
            }

            if (!string.IsNullOrWhiteSpace(filterDto.LGA))
            {
                var lga = filterDto.LGA.ToLower();
                query = query.Where(lg => lg.LGA.ToLower() == lga);
            }

            if (!string.IsNullOrWhiteSpace(filterDto.Name))
            {
                var name = filterDto.Name.ToLower();
                query = query.Where(lg => lg.Name.ToLower().Contains(name));
            }

            // Apply the query with a single execution context
            return query;
        }
        private static IQueryable<LocalGovernment> ApplySorting(
            IQueryable<LocalGovernment> query,
            string sortProperty,
            bool isDescending)
        {
            return (sortProperty, isDescending) switch
            {
                ("name", true) => query.OrderByDescending(lg => lg.Name),
                ("name", false) => query.OrderBy(lg => lg.Name),
                ("revenue", true) => query.OrderByDescending(lg => lg.CurrentRevenue),
                ("revenue", false) => query.OrderBy(lg => lg.CurrentRevenue),
                ("markets", true) => query.OrderByDescending(lg => lg.Markets.Count()),
                ("markets", false) => query.OrderBy(lg => lg.Markets.Count()),
                ("vendors", true) => query.OrderByDescending(lg => lg.Vendors.Count()),
                ("vendors", false) => query.OrderBy(lg => lg.Vendors.Count()),
                ("customers", true) => query.OrderByDescending(lg => lg.Customers.Count()),
                ("customers", false) => query.OrderBy(lg => lg.Customers.Count()),
                ("createdat", true) => query.OrderByDescending(lg => lg.CreatedAt),
                ("createdat", false) => query.OrderBy(lg => lg.CreatedAt),
                _ => query.OrderBy(lg => lg.Name)
            };
        }
    }

    // DTOs
   /* public class LocalGovernmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public decimal CurrentRevenue { get; set; }
        public int TotalMarkets { get; set; }
        public int TotalUsers { get; set; }
    }*/

}
