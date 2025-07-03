using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Services.Dtos.Levy;

namespace SabiMarket.Application.Interfaces
{
    public interface IGoodBoysService
    {
        Task<BaseResponse<GoodBoyResponseDto>> GetGoodBoyById(string goodBoyId);

        Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(CreateGoodBoyRequestDto goodBoyDto);

        Task<BaseResponse<bool>> UpdateGoodBoyProfile(string goodBoyId, UpdateGoodBoyProfileDto profileDto);

        Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(
            GoodBoyFilterRequestDto filterDto, PaginationFilter paginationFilter);

        Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId);

        Task<BaseResponse<bool>> ProcessLevyPayment(string goodBoyId, ProcessLevyPaymentDto paymentDto);

        Task<BaseResponse<TraderQRValidationResponseDto>> ValidateTraderQRCode(ScanTraderQRCodeDto scanDto);
        Task<BaseResponse<bool>> VerifyTraderPaymentStatus(string traderId);
        Task<BaseResponse<bool>> ProcessTraderLevyPayment(string traderId, ProcessLevyPaymentDto paymentDto);
        //Task<BaseResponse<bool>> UpdateTraderPayment(string traderId, ProcessLevyPaymentDto paymentDto);
       // Task<BaseResponse<GoodBoyDashboardStatsDto>> GetDashboardStats(string goodBoyId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<BaseResponse<GoodBoyDashboardStatsDto>> GetDashboardStats(
    string goodBoyId,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    string searchQuery = null,
    PaginationFilter paginationFilter = null);
        // Task<BaseResponse<IEnumerable<GoodBoyLevyPaymentResponseDto>>> GetTodayLeviesForGoodBoy(string goodBoyId);
        Task<BaseResponse<PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>>> GetTodayLeviesForGoodBoy(
    string goodBoyId,
    PaginationFilter pagination);
        Task<BaseResponse<GoodBoyLevyPaymentResponseDto>> CollectLevyPayment(LevyPaymentCreateDto levyPaymentDto);
    }

}


