using Microsoft.EntityFrameworkCore.Query;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Application.IRepositories
{
    public interface IAdminRepository : IGeneralRepository<Admin>
    {
        Task<Admin> GetAdminByIdAsync(string adminId, bool trackChanges);
        Task<Admin> GetAdminByUserIdAsync(string userId, bool trackChanges);
        Task<PaginatorDto<IEnumerable<Admin>>> GetAdminsWithPaginationAsync(
            PaginationFilter paginationFilter, bool trackChanges);
        Task<bool> AdminExistsAsync(string userId);
        void CreateAdmin(Admin admin);
        void UpdateAdmin(Admin admin);
        Task UpdateAdminStatsAsync(string adminId, int registeredLGAs, int activeChairmen, decimal totalRevenue);
        Task<Admin> GetAdminDashboardStatsAsync(string adminId);
        IQueryable<AuditLog> GetAdminAuditLogsQuery(string adminId, DateTime? startDate, DateTime? endDate);
        IIncludableQueryable<Admin, ApplicationUser> GetFilteredAdminsQuery(AdminFilterRequestDto filterDto);

        Task<ApplicationRole> GetRoleByIdAsync(string roleId, bool trackChanges = false);
        IQueryable<ApplicationRole> GetFilteredRolesQuery(RoleFilterRequestDto filterDto);
        Task<bool> RoleExistsAsync(string roleName, string excludeRoleId = null);
        Task CreateRoleAsync(ApplicationRole role);
        void UpdateRole(ApplicationRole role);
        void DeleteRole(ApplicationRole role);
        Task AddAdminToRolesAsync(string adminId, IEnumerable<string> roleIds);
        Task RemoveAdminFromRolesAsync(string adminId, IEnumerable<string> roleIds);
        Task<IEnumerable<ApplicationRole>> GetAdminRolesAsync(string adminId);
        void DeleteRolePermission(RolePermission permission);
        Task AddRolePermissionsAsync(IEnumerable<RolePermission> permissions);
        void DeleteRolePermissions(IEnumerable<RolePermission> permissions);
    }
}
