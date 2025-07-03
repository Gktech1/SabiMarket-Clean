using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.DTOs;
using SabiMarket.Domain.Enum;
using SabiMarket.Services.Dtos.Levy;
using System.Threading.Tasks;
using LevySetupResponseDto = SabiMarket.Application.DTOs.Requests.LevySetupResponseDto;

namespace SabiMarket.Application.IServices
{
    public interface IChairmanService
    {
        Task<BaseResponse<LGAResponseDto>> GetLocalGovernmentById(string id);
        Task<BaseResponse<TraderResponseDto>> CreateTrader(CreateTraderRequestDto request);
        Task<BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>> GetLocalGovernments(
       LGAFilterRequestDto filterDto,
       PaginationFilter paginationFilter);

        Task<BaseResponse<PaginatorDto<IEnumerable<LGResponseDto>>>> GetLocalGovernmentAreas(
           string searchTerm,
             PaginationFilter paginationFilter);
        Task<BaseResponse<ChairmanResponseDto>> GetChairmanById(string chairmanId);
        Task<BaseResponse<PaginatorDto<IEnumerable<LevyPaymentWithTraderDto>>>> GetLevyPayments(
      PaymentPeriodEnum? period,
      string searchQuery,
      PaginationFilter paginationFilter);
        Task<BaseResponse<ChairmanResponseDto>> CreateChairman(CreateChairmanRequestDto chairmanDto);

        Task<BaseResponse<ChairmanDashboardStatsDto>> GetChairmanDashboardStats(string chairmanId);

        Task<BaseResponse<PaginatorDto<List<AssistOfficerListDto>>>> GetAssistOfficers(
              PaginationFilter pagination,
              string searchTerm = "",
              string status = "Active");
        Task<BaseResponse<bool>> UpdateChairmanProfile(string chairmanId, UpdateProfileDto profileDto);
        Task<BaseResponse<AdminDashboardResponse>> GetChairmen(string? searchTerm, PaginationFilter paginationFilter);
        Task<BaseResponse<IEnumerable<MarketResponseDto>>> GetAllMarkets(string localgovermentId = null, string searchQuery = null);
        // Task<BaseResponse<PaginatorDto<IEnumerable<ChairmanResponseDto>>>> GetChairmen(string? searchTerm, PaginationFilter paginationFilter);
        //Task<BaseResponse<IEnumerable<MarketResponseDto>>> GetAllMarkets(string localgovermentId = null);
        Task<BaseResponse<DashboardMetricsResponseDto>> GetDashboardMetrics();
        Task<BaseResponse<bool>> AssignCaretakerToMarket(string marketId, string caretakerId);
        Task<BaseResponse<bool>> AssignCaretakerToChairman(string chairmanId, string caretakerId);
        //Task<BaseResponse<IEnumerable<CaretakerResponseDto>>> GetAllCaretakers();
        Task<BaseResponse<IEnumerable<ReportResponseDto>>> GetChairmanReports(string chairmanId);
        Task<BaseResponse<AssistantOfficerResponseDto>> UpdateAssistantOfficer(string officerId, UpdateAssistantOfficerRequestDto request);
        Task<BaseResponse<bool>> UnblockAssistantOfficer(string officerId);
        Task<BaseResponse<bool>> BlockAssistantOfficer(string officerId);
        Task<BaseResponse<AssistantOfficerResponseDto>> CreateAssistantOfficer(CreateAssistantOfficerRequestDto officerDto);
        Task<BaseResponse<AssistantOfficerResponseDto>> GetAssistantOfficerById(string officerId);
        Task<BaseResponse<bool>> DeleteAssistCenterOfficerByAdmin(string officerId);
        Task<BaseResponse<MarketResponseDto>> CreateMarket(CreateMarketRequestDto request);
        Task<BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>> SearchLevyPayments(
    string chairmanId,
    string searchQuery,
    PaginationFilter paginationFilter);
        Task<BaseResponse<bool>> UpdateMarket(string marketId, UpdateMarketRequestDto request);
        Task<BaseResponse<bool>> DeleteMarket(string marketId);
        Task<BaseResponse<MarketDetailsDto>> GetMarketDetails(string marketId);
        Task<BaseResponse<IEnumerable<MarketResponseDto>>> SearchMarkets(string searchTerm);
        Task<BaseResponse<MarketRevenueDto>> GetMarketRevenue(string marketId);

        // Trader Management
        Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetTraders(string marketId, PaginationFilter filter);
        Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId);
        Task<BaseResponse<QRCodeResponseDto>> GenerateTraderQRCode(string traderId);

        // Settings Management
        Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogDto>>>> GetAuditLogs(PaginationFilter filter);
        Task<BaseResponse<bool>> ConfigureLevySetup(LevySetupRequestDto request);
        Task<BaseResponse<bool>> UpdateLevySetup(UpdateLevySetupRequestDto request);
        Task<BaseResponse<IEnumerable<LevySetupResponseDto>>> GetLevySetups();

        // Report Generation
        Task<BaseResponse<ReportMetricsDto>> GetReportMetrics(DateTime startDate, DateTime endDate);
        Task<BaseResponse<byte[]>> ExportReport(ReportExportRequestDto request);
        Task<BaseResponse<ReportMetricsDto>> GetReportMetrics();
        Task<BaseResponse<DashboardMetricsResponseDto>> GetDailyMetricsChange();

        // Market Analytics
        Task<BaseResponse<MarketComplianceDto>> GetMarketComplianceRates(string marketId);
        Task<BaseResponse<MarketRevenueDto>> GetMarketRevenue(string marketId, DateRangeDto dateRange);

        // New levy management methods
        Task<BaseResponse<LevyResponseDto>> CreateLevy(CreateLevyRequestDto request);
        Task<BaseResponse<bool>> UpdateLevy(string levyId, UpdateLevyRequestDto request);
        Task<BaseResponse<bool>> DeleteLevy(string levyId);
        Task<BaseResponse<LevyResponseDto>> GetLevyById(string levyId);
        Task<BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>> GetAllLevies(string chairmanId, PaginationFilter filter);
        Task<BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>> GetMarketLevies(string marketId, PaginationFilter paginationFilter);
        Task<BaseResponse<bool>> DeleteChairmanByAdmin(string chairmanId);

        Task<BaseResponse<LocalGovernmentWithUsersResponseDto>> GetLocalGovernmentWithUsersByUserId(string userId);

        Task<BaseResponse<TraderDashboardResponseDto>> GetTraderDashboard(string traderId);
        Task<BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>> GetAllLevyPaymentsForTrader(
            string traderId,
            DateTime? fromDate,
            DateTime? toDate,
            string searchQuery,
            PaginationFilter pagination);
        Task<BaseResponse<TraderLevyPaymentDto>> RecordLevyPayment(LevyPaymentCreateDto paymentDto);

        Task<BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>> GetLevyPaymentsForTrader(
         string traderId,
         DateTime? fromDate,
         DateTime? toDate,
         string searchQuery,
         PaginationFilter pagination);

        Task<BaseResponse<IEnumerable<CaretakerResponseDto>>> GetAllCaretakers(string userId);

        Task<BaseResponse<TraderQRValidationResponseDto>> ValidateTraderQRCode(ScanTraderQRCodeDto scanDto);

        Task<BaseResponse<bool>> ProcessTraderLevyPayment(string traderId, ProcessAsstOfficerLevyPaymentDto paymentDto);
        //Task<BaseResponse<bool>> UpdateTraderMarket(string officerId, string traderId, UpdateTraderMarketDto traderDto);

        Task<BaseResponse<bool>> UpdateTraderMarket(string officerId, string tin, UpdateTraderMarketDto traderDto);

        Task<BaseResponse<bool>> UpdateLevyPaymentFrequency(string officerId, UpdateLevyFrequencyDto levyDto);

    }
}
