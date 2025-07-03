using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Services.SowFoodLinkUpServices
{
    public class SowFoodStaffService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SowFoodStaffService(IRepositoryManager repositoryManager, IHttpContextAccessor httpContextAccessor)
        {
            _repositoryManager = repositoryManager;
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }
        private async Task CreateAuditLog(string activity, string details, string module = "SowFoodStaff")
        {
            var userId = "";
            var auditLog = new AuditLog
            {
                UserId = userId,
                Activity = activity,
                Module = module,
                Details = details,
                IpAddress = GetCurrentIpAddress()
            };
            auditLog.SetDateTime(DateTime.UtcNow);

            _repositoryManager.AuditLogRepository.Create(auditLog);
            await _repositoryManager.SaveChangesAsync();
        }
        public async Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Trader Details Query",
                    $"CorrelationId: {correlationId} - Fetching trader: {traderId}",
                    "Trader Management"
                );

                var trader = new { };
                if (trader == null)
                {
                    await CreateAuditLog(
                        "Trader Details Query Failed",
                        $"CorrelationId: {correlationId} - Trader not found",
                        "Trader Management"
                    );
                    return ResponseFactory.Fail<TraderDetailsDto>(new NotFoundException("Trader not found"), "Not found");
                }

                var traderDto = new TraderDetailsDto();//mapp

                await CreateAuditLog(
                    "Trader Details Retrieved",
                    $"CorrelationId: {correlationId} - Trader details retrieved successfully",
                    "Trader Management"
                );

                return ResponseFactory.Success(traderDto, "Trader details retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Trader Details Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Trader Management"
                );
                return ResponseFactory.Fail<TraderDetailsDto>(ex, "An unexpected error occurred");
            }
        }
     /*   public async Task<BaseResponse<string>> CreateStaff(CreateSowFoodStaffDto dto)
        {

            var subscription = new SowFoodCompanyStaff
            {
                FullName = dto.FullName,
                EmailAddress = dto.EmailAddress,
                IsActive = true,
                ImageUrl = dto.ImageUrl,
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                StaffId = "nextStaffId",
                SowFoodCompanyId = dto.SowFoodCompanyId
            };

            _repositoryManager.StaffRepository.AddStaff(subscription);
            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success", "Subscription Created Successfully.");
        }*/

        public async Task<BaseResponse<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>>> GetAllStaffs(PaginationFilter filter)
        {
            var subscriptionPlan = await _repositoryManager.StaffRepository.GetPagedStaff(filter);
            if (subscriptionPlan == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>>(new NotFoundException("No Record Found."), "Record not found.");
            }

            return ResponseFactory.Success<PaginatorDto<IEnumerable<SowFoodCompanyStaff>>>(subscriptionPlan);
        }
        public async Task<BaseResponse<SowFoodCompanyStaff>> GetStaffById(string Id)
        {
            var subscriptionPlan = await _repositoryManager.StaffRepository.GetStaffById(Id, false);
            if (subscriptionPlan == null)
            {
                return ResponseFactory.Fail<SowFoodCompanyStaff>(new NotFoundException("No Record Found."), "Record not found.");
            }

            return ResponseFactory.Success(subscriptionPlan);
        }

        public async Task<BaseResponse<string>> UpdateStaff(UpdateSowFoodStaffDto dto)
        {
            var plan = await _repositoryManager.StaffRepository.GetStaffById(dto.Id, true);
            if (plan == null)
            {
                return ResponseFactory.Fail<string>(new NotFoundException("No Record Found."), "Record not found.");
            }


            await _repositoryManager.SaveChangesAsync();
            return ResponseFactory.Success("Success");

        }
    }
}
