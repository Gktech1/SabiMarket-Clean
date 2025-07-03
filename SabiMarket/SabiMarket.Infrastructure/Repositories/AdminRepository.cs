using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories;
using System.Linq;

public class AdminRepository : GeneralRepository<Admin>, IAdminRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AdminRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Admin> GetAdminByIdAsync(string adminId, bool trackChanges)
    {
        return await FindByCondition(a => a.UserId == adminId, trackChanges)
            .Include(a => a.User)
            .FirstOrDefaultAsync();
    }

    public async Task<Admin> GetAdminByUserIdAsync(string userId, bool trackChanges)
    {
        return await FindByCondition(a => a.UserId == userId, trackChanges)
            .Include(a => a.User)
            .FirstOrDefaultAsync();
    }

    public async Task<PaginatorDto<IEnumerable<Admin>>> GetAdminsWithPaginationAsync(
        PaginationFilter paginationFilter, bool trackChanges)
    {
        return await FindPagedByCondition(
            paginationFilter: paginationFilter,
            trackChanges: trackChanges,
            orderBy: query => query.OrderBy(a => a.CreatedAt));
    }

    public async Task<bool> AdminExistsAsync(string userId)
    {
        return await FindByCondition(a => a.UserId == userId, trackChanges: false)
            .AnyAsync();
    }

    public void CreateAdmin(Admin admin) => Create(admin);

    public void UpdateAdmin(Admin admin) => Update(admin);

    public async Task UpdateAdminStatsAsync(string adminId, int registeredLGAs, int activeChairmen, decimal totalRevenue)
    {
        var admin = await GetAdminByIdAsync(adminId, trackChanges: true);
        if (admin != null)
        {
            admin.RegisteredLGAs = registeredLGAs;
            admin.ActiveChairmen = activeChairmen;
            admin.TotalRevenue = totalRevenue;
            admin.StatsLastUpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<Admin> GetAdminDashboardStatsAsync(string adminId)
    {
        return await GetAdminByIdAsync(adminId, trackChanges: false);
    }


    public IQueryable<AuditLog> GetAdminAuditLogsQuery(string adminId, DateTime? startDate, DateTime? endDate)
    {
        var query = _dbContext.AuditLogs
            .Include(a => a.User)
            .Where(a => a.UserId == adminId ||
                       (a.Activity.Contains("admin") && a.Details.Contains(adminId)));

        // Apply date filtering if provided
        if (startDate.HasValue)
        {
            var start = startDate.Value.Date; // Set to start of day
            query = query.Where(a => a.Date >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1); // Set to end of day
            query = query.Where(a => a.Date <= end);
        }

        // Order by most recent first
        return query.OrderByDescending(a => a.Date)
                   .ThenByDescending(a => a.Time);
    }

    public IIncludableQueryable<Admin, ApplicationUser> GetFilteredAdminsQuery(AdminFilterRequestDto filterDto)
    {
        // Start with includable query
        var query = _dbContext.Set<Admin>().Include(a => a.User);

        // Build the base query
        if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
        {
            var searchTerm = filterDto.SearchTerm.ToLower();
            return query.Where(a =>
                a.User.FirstName.ToLower().Contains(searchTerm) ||
                a.User.LastName.ToLower().Contains(searchTerm) ||
                a.User.Email.ToLower().Contains(searchTerm) ||
                a.Position.ToLower().Contains(searchTerm) ||
                a.Department.ToLower().Contains(searchTerm))
                .Include(a => a.User);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.Department))
        {
            query = query.Where(a => a.Department == filterDto.Department)
                        .Include(a => a.User);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.AdminLevel))
        {
            query = query.Where(a => a.AdminLevel == filterDto.AdminLevel)
                        .Include(a => a.User);
        }

        if (filterDto.IsActive.HasValue)
        {
            query = query.Where(a => a.User.IsActive == filterDto.IsActive.Value)
                        .Include(a => a.User);
        }

        if (filterDto.StartDate.HasValue)
        {
            var startDate = filterDto.StartDate.Value.Date;
            query = query.Where(a => a.User.CreatedAt.Date >= startDate)
                        .Include(a => a.User);
        }

        if (filterDto.EndDate.HasValue)
        {
            var endDate = filterDto.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.User.CreatedAt.Date <= endDate)
                        .Include(a => a.User);
        }

        // Return ordered query with include
        return query.OrderByDescending(a => a.User.CreatedAt)
                   .Include(a => a.User);
    }

    public async Task<ApplicationRole> GetRoleByIdAsync(string roleId, bool trackChanges)
    {
        var query = !trackChanges ?
            _dbContext.Roles.AsNoTracking() :
            _dbContext.Roles;

        return await query
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public void DeleteRolePermission(RolePermission permission)
    {
        _dbContext.Set<RolePermission>().Remove(permission);
    }

    public void DeleteRole(ApplicationRole role)
    {
        _dbContext.Roles.Remove(role);
    }

 /*   public void DeleteRole(ApplicationRole role)
    {
        try
        {
            // Get all permissions for this role
            var permissions = _dbContext.Set<RolePermission>()
                .Where(rp => rp.RoleId == role.Id)
                .ToList();

            // Delete permissions first
            if (permissions.Any())
            {
                _dbContext.Set<RolePermission>().RemoveRange(permissions);
                _dbContext.SaveChanges();
            }

            // Now delete the role
            _dbContext.Roles.Remove(role);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting role: {ex.Message}", ex);
        }
    }*/

    /*public async Task<ApplicationRole> GetRoleByIdAsync(string roleId, bool trackChanges)
    {
        var query = !trackChanges ?
            _dbContext.Roles.AsNoTracking() :
            _dbContext.Roles;

        return await query
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }*/

    public IQueryable<ApplicationRole> GetFilteredRolesQuery(RoleFilterRequestDto filterDto)
    {
        var query = _dbContext.Roles
            .Include(r => r.Permissions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
        {
            var searchTerm = filterDto.SearchTerm.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(searchTerm) ||
                r.Description.ToLower().Contains(searchTerm));
        }

        return query.OrderBy(r => r.Name);
    }

    public async Task<bool> RoleExistsAsync(string roleName, string excludeRoleId = null)
    {
        var query = _dbContext.Roles.Where(r => r.NormalizedName == roleName.ToUpper());

        if (!string.IsNullOrEmpty(excludeRoleId))
        {
            query = query.Where(r => r.Id != excludeRoleId);
        }

        return await query.AnyAsync();
    }

    public async Task CreateRoleAsync(ApplicationRole role)
    {
        await _dbContext.Roles.AddAsync(role);
    }

    public void UpdateRole(ApplicationRole role)
    {
        _dbContext.Roles.Update(role);
    }

  /*  public void DeleteRole(ApplicationRole role)
    {
        _dbContext.Roles.Remove(role);
    }*/

    public async Task AddAdminToRolesAsync(string adminId, IEnumerable<string> roleIds)
    {
        var userRoles = roleIds.Select(roleId => new IdentityUserRole<string>
        {
            UserId = adminId,
            RoleId = roleId
        });

        await _dbContext.UserRoles.AddRangeAsync(userRoles);
    }

    public async Task RemoveAdminFromRolesAsync(string adminId, IEnumerable<string> roleIds)
    {
        var userRoles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == adminId && roleIds.Contains(ur.RoleId))
            .ToListAsync();

        _dbContext.UserRoles.RemoveRange(userRoles);
    }

    public async Task<IEnumerable<ApplicationRole>> GetAdminRolesAsync(string adminId)
    {
        return await _dbContext.UserRoles
            .Where(ur => ur.UserId == adminId)
            .Join(_dbContext.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r)
            .Include(r => r.Permissions)
            .ToListAsync();
    }

    public async Task AddRolePermissionsAsync(IEnumerable<RolePermission> permissions)
    {
        await _dbContext.Set<RolePermission>().AddRangeAsync(permissions);
    }

    public void DeleteRolePermissions(IEnumerable<RolePermission> permissions)
    {
        _dbContext.Set<RolePermission>().RemoveRange(permissions);
    }
}
