using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.Interfaces;
using SabiMarket.Services.Dtos.Levy;
using System.Security.Claims;
using SabiMarket.Infrastructure.Helpers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = PolicyNames.RequireMarketStaff)]
public class GoodBoysController : ControllerBase
{
    private readonly IGoodBoysService _goodBoysService;
    private readonly ILogger<GoodBoysController> _logger;
    private readonly ICurrentUserService _currentUser;

    public GoodBoysController(IGoodBoysService goodBoysService, ILogger<GoodBoysController> logger, ICurrentUserService currentUser = null)
    {
        _goodBoysService = goodBoysService;
        _logger = logger;
        _currentUser = currentUser;
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireMarketManagement)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateGoodBoy([FromBody] CreateGoodBoyRequestDto request)
    {
        var response = await _goodBoysService.CreateGoodBoy(request);
        return !response.IsSuccessful ? BadRequest(response) : CreatedAtAction(nameof(GetGoodBoyById), new { goodBoyId = response.Data.Id }, response);
    }

    [HttpGet("{goodBoyId}")]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGoodBoyById(string goodBoyId)
    {
        var response = await _goodBoysService.GetGoodBoyById(goodBoyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPut("{goodBoyId}/profile")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile(string goodBoyId, [FromBody] UpdateGoodBoyProfileDto request)
    {
        var response = await _goodBoysService.UpdateGoodBoyProfile(goodBoyId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireMarketManagement)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGoodBoys([FromQuery] GoodBoyFilterRequestDto filter, [FromQuery] PaginationFilter pagination)
    {
        var response = await _goodBoysService.GetGoodBoys(filter, pagination);
        return Ok(response);
    }

    [HttpPost("scan-qr")]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<TraderQRValidationResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ScanTraderQRCode([FromBody] ScanTraderQRCodeDto request)
    {
        var response = await _goodBoysService.ValidateTraderQRCode(request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("traders/{traderId}")]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<TraderDetailsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTraderDetails(string traderId)
    {
        var response = await _goodBoysService.GetTraderDetails(traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("traders/{traderId}/payment-status")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyTraderPaymentStatus(string traderId)
    {
        var response = await _goodBoysService.VerifyTraderPaymentStatus(traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPost("updatetraderpayment/{traderId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessPayment(string traderId, [FromBody] ProcessLevyPaymentDto request)
    {
        var response = await _goodBoysService.ProcessTraderLevyPayment(traderId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("dashboard/stats")]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyDashboardStatsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyDashboardStatsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyDashboardStatsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDashboardStats([FromQuery] string goodboyId, [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] string searchQuery = null,
    [FromQuery] PaginationFilter paginationFilter = null)
    {
   
        // Get the current user's ID
        //var userId = _currentUser.GetUserId();

        // Get dashboard statistics for the GoodBoy
        var result = await _goodBoysService.GetDashboardStats(goodboyId, fromDate, toDate, searchQuery, paginationFilter);
        return result.IsSuccessful ? Ok(result) : BadRequest(result);
    }

    [HttpGet("dashboard/today-levies")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTodayLeviesForGoodBoy(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
    {
        var userId = _currentUser.GetUserId();
        var pagination = new PaginationFilter
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _goodBoysService.GetTodayLeviesForGoodBoy(userId, pagination);

        if (result.IsSuccessful)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("dashboard/collect-levy")]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyLevyPaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyLevyPaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyLevyPaymentResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CollectLevy([FromBody] LevyPaymentCreateDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get the current user's ID
        var userId = _currentUser.GetUserId();

        // Set the GoodBoy ID for the levy payment
        request.GoodBoyId = userId;

        // Collect the levy payment
        var result = await _goodBoysService.CollectLevyPayment(request);
        return result.IsSuccessful ? Ok(result) : BadRequest(result);
    }
}