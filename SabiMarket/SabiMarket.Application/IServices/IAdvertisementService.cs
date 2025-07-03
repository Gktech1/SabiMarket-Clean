using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.Services.Interfaces
{
    public interface IAdvertisementService
    {
        Task<BaseResponse<AdvertisementResponseDto>> CreateAdvertisement(CreateAdvertisementRequestDto request);
        Task<BaseResponse<AdvertisementResponseDto>> UpdateAdvertisement(UpdateAdvertisementRequestDto request);
        Task<BaseResponse<AdvertisementDetailResponseDto>> GetAdvertisementById(string id);
        Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetAdvertisements(
            AdvertisementFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<AdvertisementResponseDto>> UpdateAdvertisementStatus(string id, string status);
        Task<BaseResponse<AdvertisementResponseDto>> DeleteAdvertisement(string id);
        Task<BaseResponse<AdvertisementResponseDto>> UploadPaymentProof(UploadPaymentProofRequestDto request, IFormFile proofImage);
        Task<BaseResponse<AdvertisementResponseDto>> ApprovePayment(string advertisementId);
        Task<BaseResponse<AdvertisementResponseDto>> RejectPayment(string advertisementId, string reason);
        Task<BaseResponse<AdvertisementResponseDto>> RestoreAdvertisement(string id);
        Task<BaseResponse<AdvertisementResponseDto>> ArchiveAdvertisement(string id);
        Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetArchivedAdvertisements(
     AdvertisementFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetSubmittedAdvertisements(
       AdvertisementFilterRequestDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<PaginatorDto<IEnumerable<VendorAdvertisementSummaryDto>>>> GetVendorAdvertisementSummaries(
            VendorFilterDto filter, PaginationFilter paginationFilter);
        Task<BaseResponse<BulkOperationResultDto>> BulkRejectAdvertisements(List<string> advertisementIds, string reason);
        Task<BaseResponse<BulkOperationResultDto>> BulkApproveAdvertisements(List<string> advertisementIds);
        Task<BaseResponse<AdvertisementAnalyticsDto>> GetAdvertisementAnalytics(AnalyticsFilterDto filter);
        Task<BaseResponse<AdvertisementPerformanceDto>> GetAdvertisementPerformance(string advertisementId);
        Task<BaseResponse<List<AdvertisementAlertDto>>> GetAdvertisementAlerts();
        Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetAllAdvertisementsForAdmin(
            AdminAdvertisementFilterDto filterDto, PaginationFilter paginationFilter);
        Task<BaseResponse<AdvertisementDashboardStatsDto>> GetAdvertDashboardStats();
        Task<BaseResponse<PaginatorDto<IEnumerable<PaymentVerificationDto>>>> GetPendingPaymentVerifications(
            PaginationFilter paginationFilter);

    }
}