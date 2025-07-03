using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.IServices
{
    public interface IAdminService
    {
        Task<BaseResponse<AdminResponseDto>> GetAdminById(string adminId);
        Task<BaseResponse<AdminResponseDto>> CreateAdmin(CreateAdminRequestDto adminDto);
        Task<BaseResponse<bool>> UpdateAdminProfile(string adminId, UpdateAdminProfileDto profileDto);
        Task<BaseResponse<PaginatorDto<IEnumerable<AdminResponseDto>>>> GetAdmins(
            AdminFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<AdminDashboardStatsDto>> GetDashboardStats(string adminId);
        Task<BaseResponse<bool>> UpdateDashboardAccess(string adminId, UpdateAdminAccessDto accessDto);
        Task<BaseResponse<bool>> DeactivateAdmin(string adminId);
        Task<BaseResponse<bool>> ReactivateAdmin(string adminId);
        Task<BaseResponse<DashboardReportDto>> GetDashboardReportDataAsync(
         string lgaFilter = null,
         string marketFilter = null,
         int? year = null,
         TimeFrame timeFrame = TimeFrame.ThisWeek);
        Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>> GetAdminAuditLogs(
            string adminId, DateTime? startDate, DateTime? endDate, PaginationFilter paginationFilter);
        Task<BaseResponse<bool>> DeleteRole(string roleId);
        Task<BaseResponse<RoleResponseDto>> UpdateRole(string roleId, UpdateRoleRequestDto updateRoleDto);
        Task<BaseResponse<RoleResponseDto>> CreateRole(CreateRoleRequestDto createRoleDto);
        Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
       RoleFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<RoleResponseDto>> GetRoleById(string roleId);
        Task<BaseResponse<TeamMemberResponseDto>> CreateTeamMember(CreateTeamMemberRequestDto requestDto);
        Task<BaseResponse<TeamMemberResponseDto>> UpdateTeamMember(string memberId, UpdateTeamMemberRequestDto requestDto);
        Task<BaseResponse<TeamMemberResponseDto>> GetTeamMemberById(string memberId);
        Task<BaseResponse<bool>> DeleteTeamMember(string memberId);
        Task<BaseResponse<PaginatorDto<IEnumerable<TeamMemberResponseDto>>>> GetTeamMembers(
         TeamMemberFilterRequestDto filterDto,
         PaginationFilter paginationFilter);
        Task<BaseResponse<byte[]>> ExportReport(ReportExportRequestDto request);
        Task<BaseResponse<PaginatorDto<IEnumerable<UserResponseDto>>>> GetAllUsers(
    UserFilterRequestDto filterDto,
    PaginationFilter paginationFilter);
        Task<BaseResponse<AssignRoleResponseDto>> AssignUserRoleAndPermissions(string userId, string roleId, List<PermissionDto> permissions, bool removeExistingRoles = false);
    }
}


