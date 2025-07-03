using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class AuditLogRepository : GeneralRepository<AuditLog>, IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddAuditLog(AuditLog auditLog) => Create(auditLog);

        public async Task<PaginatorDto<IEnumerable<AuditLog>>> GetAuditLogsAsync(AuditLogFilter filter, bool trackChanges)
        {
            var query = FindAll(trackChanges);

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(x => x.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(x => x.Date <= filter.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.User))
                query = query.Where(x =>
                    x.User != null &&
                    (x.User.FirstName.Contains(filter.User) ||
                     x.User.LastName.Contains(filter.User)));

            if (!string.IsNullOrWhiteSpace(filter.Activity))
                query = query.Where(x => x.Activity.Contains(filter.Activity));

            if (!string.IsNullOrWhiteSpace(filter.IpAddress))
                query = query.Where(x => x.IpAddress == filter.IpAddress);

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "date" => filter.SortDescending ?? false
                        ? query.OrderByDescending(x => x.Date)
                        : query.OrderBy(x => x.Date),
                    "user" => filter.SortDescending ?? false
                        ? query.OrderByDescending(x => x.User.FirstName) // Sort by FirstName or another field
                        : query.OrderBy(x => x.User.FirstName),
                    "activity" => filter.SortDescending ?? false
                        ? query.OrderByDescending(x => x.Activity)
                        : query.OrderBy(x => x.Activity),
                    _ => query.OrderByDescending(x => x.Date) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.Date)
                            .ThenByDescending(x => x.Time);
            }

            // Apply pagination
            var paginationFilter = new PaginationFilter
            {
                PageNumber = filter.PageNumber ?? 1,
                PageSize = filter.PageSize ?? 10
            };

            return await query.Paginate(paginationFilter);
        }


       /* public async Task<PaginatorDto<IEnumerable<AuditLog>>> GetAuditLogsAsync(AuditLogFilter filter, bool trackChanges)
        {
            var query = FindAll(trackChanges);

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(x => x.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(x => x.Date <= filter.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.User))
                query = query.Where(x => x.User.Contains(filter.User));

            if (!string.IsNullOrWhiteSpace(filter.Activity))
                query = query.Where(x => x.Activity.Contains(filter.Activity));

            if (!string.IsNullOrWhiteSpace(filter.IpAddress))
                query = query.Where(x => x.IpAddress == filter.IpAddress);

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                query = filter.SortBy.ToLower() switch
                {
                    "date" => filter.SortDescending ?? false
                        ? query.OrderByDescending(x => x.Date)
                        : query.OrderBy(x => x.Date),
                    "user" => filter.SortDescending ?? false
                        ? query.OrderByDescending(x => x.User)
                        : query.OrderBy(x => x.User),
                    "activity" => filter.SortDescending ?? false
                        ? query.OrderByDescending(x => x.Activity)
                        : query.OrderBy(x => x.Activity),
                    _ => query.OrderByDescending(x => x.Date) // Default sort
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.Date)
                            .ThenByDescending(x => x.Time);
            }

            // Apply pagination
            var paginationFilter = new PaginationFilter
            {
                PageNumber = filter.PageNumber ?? 1,
                PageSize = filter.PageSize ?? 10
            };

            return await query.Paginate(paginationFilter);
        }
*/
        public async Task<IEnumerable<AuditLog>> GetAllAuditLogs(bool trackChanges) =>
            await FindAll(trackChanges).ToListAsync();

        public async Task<AuditLog> GetAuditLogById(string id, bool trackChanges) =>
            await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync();

        public async Task<PaginatorDto<IEnumerable<AuditLog>>> GetPagedAuditLogs(PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Time)
                .Paginate(paginationFilter);
        }

        /*  public async Task<PaginatorDto<IEnumerable<AuditLog>>> SearchAuditLogs(string searchString, PaginationFilter paginationFilter)
          {
              return await FindAll(false)
                  .Where(a => a.Contains(searchString) ||
                             a.Activity.Contains(searchString) ||
                             a.Date.ToString().Contains(searchString))
                  .OrderByDescending(x => x.Date)
                  .ThenByDescending(x => x.Time)
                  .Paginate(paginationFilter);
          }*/

        public async Task<PaginatorDto<IEnumerable<AuditLog>>> SearchAuditLogs(string searchString, PaginationFilter paginationFilter)
        {
            return await FindAll(false)
                .Where(a =>
                    (a.UserId != null && a.UserId.Contains(searchString)) ||
                    (a.Activity != null && a.Activity.Contains(searchString)) ||
                    (a.Module != null && a.Module.Contains(searchString)) ||
                    (a.Details != null && a.Details.Contains(searchString)) ||
                    (a.IpAddress != null && a.IpAddress.Contains(searchString)))
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Time)
                .Paginate(paginationFilter);
        }


        public async Task<IEnumerable<AuditLog>> GetUserActivity(string userId, DateTime startDate, DateTime endDate)
        {
            return await FindByCondition(
                x => x.UserId == userId &&
                x.Date >= startDate &&
                x.Date <= endDate,
                trackChanges: false)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Time)
                .ToListAsync();
        }
    }
}