using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities;

public interface IAuditLogRepository : IGeneralRepository<AuditLog>
{
    void AddAuditLog(AuditLog auditLog);
    Task<PaginatorDto<IEnumerable<AuditLog>>> GetAuditLogsAsync(AuditLogFilter filter, bool trackChanges);
    Task<IEnumerable<AuditLog>> GetAllAuditLogs(bool trackChanges);
    Task<AuditLog> GetAuditLogById(string id, bool trackChanges);
    Task<PaginatorDto<IEnumerable<AuditLog>>> GetPagedAuditLogs(PaginationFilter paginationFilter);
    Task<PaginatorDto<IEnumerable<AuditLog>>> SearchAuditLogs(string searchString, PaginationFilter paginationFilter);
    Task<IEnumerable<AuditLog>> GetUserActivity(string userId, DateTime startDate, DateTime endDate);
}