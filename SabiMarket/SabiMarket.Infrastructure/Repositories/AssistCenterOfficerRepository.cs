using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;
using System.Linq.Expressions;
using System.Security.Policy;

namespace SabiMarket.Infrastructure.Repositories
{
    public class AssistCenterOfficerRepository : GeneralRepository<AssistCenterOfficer>, IAssistCenterOfficerRepository
    {
        private readonly ApplicationDbContext _context;
        public AssistCenterOfficerRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public void AddAssistCenterOfficer(AssistCenterOfficer assistCenter) => Create(assistCenter);

        public async Task<PaginatorDto<IEnumerable<AssistCenterOfficer>>> GetAssistantOfficersAsync(
    string chairmanId, PaginationFilter paginationFilter, bool trackChanges)
        {
            return await FindPagedByCondition(
                expression: a => a.ChairmanId == chairmanId,
                paginationFilter: paginationFilter,
                trackChanges: trackChanges,
                orderBy: query => query.OrderBy(a => a.CreatedAt));
        }

       public void UpdateAssistCenterOfficer(AssistCenterOfficer assistCenter) => Update(assistCenter);

      /*  public void UpdateAssistCenterOfficer(AssistCenterOfficer assistCenter)
        {
            _context.Attach(assistCenter);
            _context.Entry(assistCenter).State = EntityState.Modified;
        }*/

        public async Task<IEnumerable<AssistCenterOfficer>> GetAllAssistCenterOfficer(bool trackChanges) => await FindAll(trackChanges).ToListAsync();

        public async Task<AssistCenterOfficer> GetByIdAsync(string officerId, bool trackChanges)
        {
            return await FindByCondition(a => a.Id == officerId, trackChanges)
                .FirstOrDefaultAsync();
        }

        public async Task<AssistCenterOfficer> GetAssistantOfficerByIdAsync(string officerId, bool trackChanges)
        {
            var query = FindByCondition(a => a.Id == officerId, trackChanges);

            // Include related entities one by one
            query = query.Include(a => a.User);
            query = query.Include(a => a.Market);
            query = query.Include(a => a.LocalGovernment);

            // Disable tracking if needed
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<AssistCenterOfficer> GetAssistantOfficerByUserIdAsync(string userId, bool trackChanges)
        {
            var query = FindByCondition(a => a.UserId == userId, trackChanges);

            // Include related entities one by one
            query = query.Include(a => a.User);
            query = query.Include(a => a.Market);
            query = query.Include(a => a.LocalGovernment);

            // Disable tracking if needed
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }
        public async Task<PaginatorDto<IEnumerable<AssistCenterOfficer>>> GetAssistOfficersAsync(
            Expression<Func<AssistCenterOfficer, bool>> expression,
            PaginationFilter paginationFilter,
            bool trackChanges)
        {
            // Get base query with condition
            var query = FindByCondition(expression, trackChanges);

            // Include all necessary related entities
            query = query
                .Include(o => o.User)
                .Include(o => o.Market)
                .Include(o => o.LocalGovernment)
                .Include(o => o.Chairman)
                .ThenInclude(c => c.User);

            // Apply ordering
            var orderedQuery = query.OrderByDescending(o => o.CreatedAt);

            // Use the Paginate extension method
            return await orderedQuery.Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<AssistCenterOfficer>>> SearchAssistOfficersAsync(
              Expression<Func<AssistCenterOfficer, bool>> baseExpression,
              string searchTerm,
              PaginationFilter paginationFilter,
              bool trackChanges)
        {
            // Get base query with condition
            var query = FindByCondition(baseExpression, trackChanges);

            // Include all necessary related entities
            query = query
                .Include(o => o.User)
                .Include(o => o.Market)
                .Include(o => o.LocalGovernment)
                .Include(o => o.Chairman)
                .ThenInclude(c => c.User);

            // Apply search filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o =>
                    o.User.FirstName.Contains(searchTerm) ||
                    o.User.LastName.Contains(searchTerm) ||
                    (o.User.FirstName + " " + o.User.LastName).Contains(searchTerm) ||
                    o.User.Email.Contains(searchTerm) ||
                    o.User.PhoneNumber.Contains(searchTerm)
                );
            }

            // Apply ordering
            var orderedQuery = query.OrderByDescending(o => o.CreatedAt);

            // Use the Paginate extension method
            return await orderedQuery.Paginate(paginationFilter);
        }

        public async Task<AssistCenterOfficer> GetAssistantOfficerWithTraderAsync(string officerId, string traderId, bool trackChanges)
        {
            var query = FindByCondition(a => a.Id == officerId, trackChanges);

            // Include related entities
            query = query
                .Include(a => a.User)
                .Include(a => a.Market)
                .Include(a => a.LocalGovernment)
                .Include(a => a.Chairman);

            // If you have traders related to officers, include them here
            // This assumes there's a relationship - adjust based on your actual entity relationships

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }

        public void DeleteAssistOfficer(AssistCenterOfficer assistCenterOfficer) => Delete(assistCenterOfficer);

    }
}


