using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IServices;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.DTOs;
using SabiMarket.Infrastructure.Helpers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = PolicyNames.RequireMarketManagement)]
public class ChairmanController : ControllerBase
{
    private readonly IChairmanService _chairmanService;
    private readonly ILogger<ChairmanController> _logger;
    private readonly ICurrentUserService _currentUser;

    public ChairmanController(
        IChairmanService chairmanService,
        ILogger<ChairmanController> logger,
        ICurrentUserService currentUser)
    {
        _chairmanService = chairmanService;
        _logger = logger;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get dashboard statistics for the logged-in chairman
    /// </summary>
    /// <returns>Dashboard statistics including trader counts, caretaker counts, and revenue metrics</returns>
    [HttpGet("chairmandashboardstats")]
    [ProducesResponseType(typeof(BaseResponse<ChairmanDashboardStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanDashboardStatsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanDashboardStatsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanDashboardStatsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmanDashboardStats(string chairmanId)
    {
        var response = await _chairmanService.GetChairmanDashboardStats(chairmanId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);

    }

    [HttpGet("getall-localgovernments")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLocalGovernments([FromQuery] LGAFilterRequestDto filterDto, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _chairmanService.GetLocalGovernments(filterDto, paginationFilter);
        return !response.IsSuccessful
            ? StatusCode(StatusCodes.Status500InternalServerError, response)
            : Ok(response);
    }

    /// <summary>
    /// Get all assistant officers with pagination, search and status filtering
    /// </summary>
    /// <param name="pageNumber">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <param name="searchTerm">Search term to filter assistant officers</param>
    /// <param name="status">Status filter (Active, Inactive, All)</param>
    /// <returns>Paginated list of assistant officers</returns>
    [HttpGet("get-assistantofficers")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<AssistOfficerListDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssistantOfficers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = "",
        [FromQuery] string status = "Active")
    {
        var paginationFilter = new PaginationFilter(pageNumber, pageSize);
        var response = await _chairmanService.GetAssistOfficers(paginationFilter, searchTerm, status);
        return Ok(response);

    }

    /// <summary>
    /// Create a new trader
    /// </summary>
    /// <param name="request">Trader creation details</param>
    /// <returns>Created trader information including default password</returns>
    [HttpPost("create-trader")]
    [Authorize(Policy = PolicyNames.RequireMarketStaff)]
    [ProducesResponseType(typeof(BaseResponse<TraderResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTrader([FromBody] CreateTraderRequestDto request)
    {
        var response = await _chairmanService.CreateTrader(request);
        return !response.IsSuccessful
            ? BadRequest(response)
            : StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Search levy payments for a chairman's market
    /// </summary>
    /// <param name="chairmanId">ID of the chairman</param>
    /// <param name="query">Search term to filter levy payments</param>
    /// <param name="pageNumber">Page number for pagination (defaults to 1)</param>
    /// <param name="pageSize">Number of items per page (defaults to 10)</param>
    /// <returns>Paginated list of levy payments matching the search criteria</returns>
    [HttpGet("searchlevies")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentDetailDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchLevyPayments(
        string chairmanId,
        [FromQuery] string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var paginationFilter = new PaginationFilter
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize > 50 ? 50 : (pageSize < 1 ? 10 : pageSize)
        };

        var response = await _chairmanService.SearchLevyPayments(chairmanId, query, paginationFilter);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("search-localgovernmentarea")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LGAResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLocalGovernmentArea([FromQuery] string search, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _chairmanService.GetLocalGovernmentAreas(search, paginationFilter);
        return !response.IsSuccessful
            ? StatusCode(StatusCodes.Status500InternalServerError, response)
            : Ok(response);
    }

    [HttpGet("localgovernment/{id}")]
    [ProducesResponseType(typeof(BaseResponse<LGAResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<LGAResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<LGAResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLocalGovernmentById(string id)
    {
        var response = await _chairmanService.GetLocalGovernmentById(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpDelete("{chairmanId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteChairman(string chairmanId)
    {
        var response = await _chairmanService.DeleteChairmanByAdmin(chairmanId);

        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }

        return Ok(response);
    }

    [HttpPost("markets")]
    [ProducesResponseType(typeof(BaseResponse<MarketResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<MarketResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<MarketResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateMarket([FromBody] CreateMarketRequestDto request)
    {
        var response = await _chairmanService.CreateMarket(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetMarketDetails), new { marketId = response.Data.Id }, response);
    }

    [HttpPut("markets/{marketId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMarket(string marketId, [FromBody] UpdateMarketRequestDto request)
    {
        var response = await _chairmanService.UpdateMarket(marketId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("markets/{marketId}/traders")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTraders(string marketId, [FromQuery] PaginationFilter filter)
    {
        var response = await _chairmanService.GetTraders(marketId, filter);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("markets/{marketId}/metrics")]
    [ProducesResponseType(typeof(BaseResponse<ReportMetricsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ReportMetricsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<ReportMetricsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetReportMetrics(string marketId, [FromQuery] DateRangeDto dateRange)
    {
        var response = await _chairmanService.GetReportMetrics(dateRange.StartDate, dateRange.EndDate);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("levypayments")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentWithTraderDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentWithTraderDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyPaymentWithTraderDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLevyPayments(
    [FromQuery] PaymentPeriodEnum? period,
    [FromQuery] string? search,
    [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _chairmanService.GetLevyPayments(period, search, paginationFilter);

        return response.IsSuccessful
            ? Ok(response)
            : BadRequest(response);
    }

    [HttpDelete("markets/{marketId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteMarket(string marketId)
    {
        var response = await _chairmanService.DeleteMarket(marketId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("markets/{marketId}/levy-setups")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyInfoResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMarketLevies(string marketId, [FromQuery] PaginationFilter filter)
    {
        var response = await _chairmanService.GetMarketLevies(marketId, filter);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("daily-metrics")]
    [ProducesResponseType(typeof(BaseResponse<DashboardMetricsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<DashboardMetricsResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDailyMetricsChange()
    {
        var response = await _chairmanService.GetDailyMetricsChange();
        return !response.IsSuccessful ?
            StatusCode(StatusCodes.Status500InternalServerError, response) :
            Ok(response);
    }

    [HttpGet("export-report")]
    [ProducesResponseType(typeof(BaseResponse<byte[]>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<byte[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<byte[]>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportReport([FromQuery] ReportExportRequestDto request)
    {
        var response = await _chairmanService.ExportReport(request);
        if (!response.IsSuccessful)
            return BadRequest(response);

        return File(
            response.Data,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"market_report_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        );
    }

    [HttpGet("markets/{marketId}/revenue")]
    [ProducesResponseType(typeof(BaseResponse<MarketRevenueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<MarketRevenueDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<MarketRevenueDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMarketRevenue(string marketId, [FromQuery] DateRangeDto dateRange)
    {
        var response = await _chairmanService.GetMarketRevenue(marketId, dateRange);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("levy-setup")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), statusCode: StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfigureLevySetup([FromBody] LevySetupRequestDto request)
    {
        var response = await _chairmanService.ConfigureLevySetup(request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpPost("update-levysetup")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), statusCode: StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLevySetup([FromBody] UpdateLevySetupRequestDto request)
    {
        var response = await _chairmanService.UpdateLevySetup(request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("levy-setups")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<LevySetupResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<LevySetupResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLevySetups()
    {
        var response = await _chairmanService.GetLevySetups();
        return !response.IsSuccessful ?
            StatusCode(StatusCodes.Status500InternalServerError, response) :
            Ok(response);
    }
    /// <summary>
    /// Get dashboard information for a trader including next payment date, total levies paid, and recent payments
    /// </summary>
    /// <param name="traderId">The ID of the trader</param>
    /// <returns>Trader dashboard information</returns>
    [HttpGet("dashboard/{traderId}")]
    [ProducesResponseType(typeof(BaseResponse<TraderDashboardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderDashboardResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<TraderDashboardResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<TraderDashboardResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTraderDashboard(string traderId)
    {
        try
        {

            _logger.LogInformation($"Getting dashboard for trader {traderId}");
            var response = await _chairmanService.GetTraderDashboard(traderId);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning($"Failed to get dashboard for trader {traderId}: {response.Message}");
                return NotFound(response);
            }

            _logger.LogInformation($"Successfully retrieved dashboard for trader {traderId}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting dashboard for trader {traderId}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ResponseFactory.Fail<TraderDashboardResponseDto>("An unexpected error occurred"));
        }
    }

    [HttpPost("scan-QRcode")]
    [ProducesResponseType(typeof(BaseResponse<TraderDashboardResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderDashboardResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<TraderDashboardResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<TraderDashboardResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ScanTraderQRCode(ScanTraderQRCodeDto scanDto)
    {
        _logger.LogInformation($"Getting Assist Officer Scan QR code for trader {scanDto.TraderId}");
        var response = await _chairmanService.ValidateTraderQRCode(scanDto);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning($"Failed to get scan qr code for trader as an assist officer => {scanDto.TraderId}: {response.Message}");
            return NotFound(response);
        }

        _logger.LogInformation($"Successfully scan qr code for trader {scanDto.TraderId}");
        return Ok(response);

    }

    [HttpPost("updatetraderpayment/{traderId}")]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessTraderLevyPayment(string traderId, [FromBody] ProcessAsstOfficerLevyPaymentDto request)
    {
        var response = await _chairmanService.ProcessTraderLevyPayment(traderId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }


    [HttpPut("changetradermarket")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeTraderMarket(string officerId, string tin, [FromBody] UpdateTraderMarketDto request)
    {
        var response = await _chairmanService.UpdateTraderMarket(officerId, tin, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpPut("updateLevypaymentfrequency")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLevyPaymentFrequency(string officerId, [FromBody] UpdateLevyFrequencyDto request)
    {
        var response = await _chairmanService.UpdateLevyPaymentFrequency(officerId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    /// <summary>
    /// Get all levy payments for a trader with pagination and filtering
    /// </summary>
    /// <param name="traderId">The ID of the trader</param>
    /// <param name="fromDate">Optional start date for filtering</param>
    /// <param name="toDate">Optional end date for filtering</param>
    /// <param name="searchQuery">Optional search query</param>
    /// <param name="pageNumber">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>Paginated list of levy payments</returns>
    [HttpGet("levies/{traderId}")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllLevyPaymentsForTrader(
        string traderId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? searchQuery,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {

        _logger.LogInformation($"Getting levy payments for trader {traderId}, " +
            $"Date Range: {fromDate?.ToString("yyyy-MM-dd") ?? "All"} to {toDate?.ToString("yyyy-MM-dd") ?? "All"}, " +
            $"Search: {searchQuery ?? "None"}, Page {pageNumber}, Size {pageSize}");

        var pagination = new PaginationFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var response = await _chairmanService.GetAllLevyPaymentsForTrader(
            traderId,
            fromDate,
            toDate,
            searchQuery,
            pagination);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning($"Failed to get levy payments for trader {traderId}: {response.Message}");
            return BadRequest(response);
        }

        _logger.LogInformation($"Successfully retrieved {response.Data?.PageItems?.Count ?? 0} levy payments for trader {traderId}");
        return Ok(response);

    }

    /// <summary>
    /// Get detailed levy payments for a trader with enhanced filtering
    /// </summary>
    /// <param name="traderId">The ID of the trader</param>
    /// <param name="fromDate">Optional start date for filtering</param>
    /// <param name="toDate">Optional end date for filtering</param>
    /// <param name="searchQuery">Optional search query</param>
    /// <param name="pageNumber">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>Paginated list of levy payments with enhanced filtering</returns>
    [HttpGet("payments/{traderId}")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<TraderLevyPaymentDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLevyPaymentsForTrader(
        string traderId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string searchQuery,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {

        _logger.LogInformation($"Getting detailed levy payments for trader {traderId}, " +
            $"Date Range: {fromDate?.ToString("yyyy-MM-dd") ?? "All"} to {toDate?.ToString("yyyy-MM-dd") ?? "All"}, " +
            $"Search: {searchQuery ?? "None"}, Page {pageNumber}, Size {pageSize}");

        var pagination = new PaginationFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var response = await _chairmanService.GetLevyPaymentsForTrader(
            traderId,
            fromDate,
            toDate,
            searchQuery,
            pagination);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning($"Failed to get detailed levy payments for trader {traderId}: {response.Message}");
            return BadRequest(response);
        }

        _logger.LogInformation($"Successfully retrieved {response.Data?.PageItems?.Count ?? 0} detailed levy payments for trader {traderId}");
        return Ok(response);

    }

    [HttpGet("traders/{traderId}/details")]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTraderDetails(string traderId)
    {
        var response = await _chairmanService.GetTraderDetails(traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("markets/{marketId}/details")]
    [ProducesResponseType(typeof(BaseResponse<MarketDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<MarketDetailsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<MarketDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMarketDetails(string marketId)
    {
        var response = await _chairmanService.GetMarketDetails(marketId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("markets/search")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MarketResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MarketResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchMarkets([FromQuery] string searchTerm)
    {
        var response = await _chairmanService.SearchMarkets(searchTerm);
        return !response.IsSuccessful ?
            StatusCode(StatusCodes.Status500InternalServerError, response) :
            Ok(response);
    }

    [HttpGet("traders/{traderId}/qrcode")]
    [ProducesResponseType(typeof(BaseResponse<QRCodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<QRCodeResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<QRCodeResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateTraderQRCode(string traderId)
    {
        var response = await _chairmanService.GenerateTraderQRCode(traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("levy")]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLevy([FromBody] CreateLevyRequestDto request)
    {
        var response = await _chairmanService.CreateLevy(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetLevyById), new { id = response.Data.Id }, response);
    }

    [HttpPut("levy/{levyId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLevy(string levyId, [FromBody] UpdateLevyRequestDto request)
    {
        var response = await _chairmanService.UpdateLevy(levyId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpDelete("levy/{levyId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteLevy(string levyId)
    {
        var response = await _chairmanService.DeleteLevy(levyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("levy/{levyId}")]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<LevyResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLevyById(string levyId)
    {
        var response = await _chairmanService.GetLevyById(levyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("chairman/{chairmanId}/levies")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<LevyResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllLevies(string chairmanId, [FromQuery] PaginationFilter filter)
    {
        var response = await _chairmanService.GetAllLevies(chairmanId, filter);
        return !response.IsSuccessful ? StatusCode(StatusCodes.Status500InternalServerError, response) : Ok(response);
    }

   /* [HttpGet("assistant-officer/{id}")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssistantOfficerById(string id)
    {
        var response = await _chairmanService.GetAssistantOfficerById(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }*/
    /// <summary>
    /// Create a new assistant officer
    /// </summary>
    /// <param name="request">Assistant officer creation details</param>
    /// <returns>Created assistant officer details including default password</returns>
    [HttpPost("createassistant-officer")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAssistantOfficer([FromBody] CreateAssistantOfficerRequestDto request)
    {
        var response = await _chairmanService.CreateAssistantOfficer(request);
        return response.IsSuccessful ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Update an existing assistant officer
    /// </summary>
    /// <param name="officerId">ID of the assistant officer to update</param>
    /// <param name="request">Assistant officer update details</param>
    /// <returns>Updated assistant officer details</returns>
    [HttpPut("updateassistant-officer/{officerId}")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAssistantOfficer(string officerId, [FromBody] UpdateAssistantOfficerRequestDto request)
    {
        var response = await _chairmanService.UpdateAssistantOfficer(officerId, request);
        return response.IsSuccessful ? Ok(response) : BadRequest(response);
    }

    [HttpGet("assistant-officer/{officerId}")]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<AssistantOfficerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssistantOfficer(string officerId)
    {
        var response = await _chairmanService.GetAssistantOfficerById(officerId);

        if (!response.IsSuccessful)
        {
            _logger.LogWarning($"Get assistant officer failed: {response.Message}");

            if (response.Error is NotFoundException)
                return NotFound(response);
            else
                return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        return Ok(response);
    }

    [HttpPatch("assistant-officer/{officerId}/unblock")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnblockAssistantOfficer(string officerid)
    {
        var response = await _chairmanService.UnblockAssistantOfficer(officerid);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }


    [HttpPatch("assistant-officer/{officerId}/block")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BlockAssistantOfficer(string officerid)
    {
        var response = await _chairmanService.BlockAssistantOfficer(officerid);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpDelete("deleteassistofficer/{officerId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAssistantOfficer(string officerId)
    {
        var response = await _chairmanService.DeleteAssistCenterOfficerByAdmin(officerId);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }

    [HttpGet("chairman/{id}/reports")]
    [ProducesResponseType(typeof(BaseResponse<ReportResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ReportResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmanReports(string id)
    {
        var response = await _chairmanService.GetChairmanReports(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("dashboard-metrics")]
    [ProducesResponseType(typeof(BaseResponse<DashboardMetricsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<DashboardMetricsResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardMetrics()
    {
        var response = await _chairmanService.GetDashboardMetrics();
        return !response.IsSuccessful ? StatusCode(StatusCodes.Status500InternalServerError, response) : Ok(response);
    }

    [HttpGet("markets")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MarketResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<MarketResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllMarkets(string? localgovernmentId, string? searchTerm)
    {
        var response = await _chairmanService.GetAllMarkets(localgovernmentId!, searchTerm!);
        return !response.IsSuccessful ? StatusCode(StatusCodes.Status500InternalServerError, response) : Ok(response);
    }

    [HttpPost("markets/{marketId}/assign-caretaker/{caretakerId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignCaretakerToMarket(string marketId, string caretakerId)
    {
        var response = await _chairmanService.AssignCaretakerToMarket(marketId, caretakerId);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("caretakers")]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<CaretakerResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IEnumerable<CaretakerResponseDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCaretakers()
    {
        var userId = _currentUser.GetUserId();
        var response = await _chairmanService.GetAllCaretakers(userId);
        return !response.IsSuccessful ? StatusCode(StatusCodes.Status500InternalServerError, response) : Ok(response);
    }

    [HttpGet("chairman/{id}")]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmanById(string id)
    {
        var response = await _chairmanService.GetChairmanById(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("create-chairman")]
    [Authorize(Policy = PolicyNames.RequireAdminOnly)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<ChairmanResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateChairman([FromBody] CreateChairmanRequestDto request)
    {
        var response = await _chairmanService.CreateChairman(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetChairmanById), new { id = response.Data.Id }, response);
    }

    [HttpPut("{id}/updatechairman-profile")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateChairmanProfile(string id, [FromBody] UpdateProfileDto profileDto)
    {
        var response = await _chairmanService.UpdateChairmanProfile(id, profileDto);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("chairmen")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdminDashboardResponse>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<AdminDashboardResponse>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChairmen([FromQuery] string? searchTerm, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _chairmanService.GetChairmen(searchTerm, paginationFilter);
        return Ok(response);
    }

    [HttpPost("{id}/assign-caretaker/{caretakerId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignCaretakerToChairman(string id, string caretakerId)
    {
        var response = await _chairmanService.AssignCaretakerToChairman(id, caretakerId);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }
}
