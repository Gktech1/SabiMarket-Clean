using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IServices;
using SabiMarket.Infrastructure.Services;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.DTOs.MarketParticipants;
using SabiMarket.Domain.Exceptions;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize(Policy = PolicyNames.RequireMarketManagement)]
public class CaretakerController : ControllerBase
{
    private readonly ICaretakerService _caretakerService;
    private readonly ILogger<CaretakerController> _logger;

    public CaretakerController(
        ICaretakerService caretakerService,
        ILogger<CaretakerController> logger)
    {
        _caretakerService = caretakerService;
        _logger = logger;
    }

    [HttpPost("createcaretaker")]
    [Authorize(Policy = PolicyNames.RequireMarketManagement)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCaretaker([FromBody] CaretakerForCreationRequestDto request)
    {
        var response = await _caretakerService.CreateCaretaker(request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    /// <summary>
    /// Update an existing caretaker
    /// </summary>
    /// <param name="caretakerId">ID of the caretaker to update</param>
    /// <param name="request">Updated caretaker data</param>
    /// <returns>Updated caretaker details</returns>
    [HttpPut("update-caretakers/{caretakerId}")]
    [Authorize(Policy = PolicyNames.RequireMarketManagement)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCaretaker(string caretakerId, [FromBody] UpdateCaretakerRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _caretakerService.UpdateCaretaker(caretakerId, request);

        if (!response.IsSuccessful)
        {
            if (response.Error is NotFoundException)
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpDelete("delete-caretaker/{caretakerId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCaretaker(string caretakerId)
    {
        var response = await _caretakerService.DeleteCaretaker(caretakerId);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<CaretakerResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCaretaker(string id)
    {
        var response = await _caretakerService.GetCaretakerById(id);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("getcaretakers")]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCaretakers([FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _caretakerService.GetCaretakers(paginationFilter);
        return Ok(response);
    }

    [HttpPost("{caretakerId}/traders/{traderId}")]
    [Authorize(Policy = PolicyNames.RequireCaretakerOnly)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTrader(string caretakerId, string traderId)
    {
        var response = await _caretakerService.AssignTraderToCaretaker(caretakerId, traderId);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpDelete("{caretakerId}/traders/{traderId}")]
    [Authorize(Policy = PolicyNames.RequireCaretakerOnly)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTrader(string caretakerId, string traderId)
    {
        var response = await _caretakerService.RemoveTraderFromCaretaker(caretakerId, traderId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("{caretakerId}/traders")]
    [Authorize(Policy = PolicyNames.RequireMarketStaff)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssignedTraders(string caretakerId, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _caretakerService.GetAssignedTraders(caretakerId, paginationFilter);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("{caretakerId}/levy-payments")]
    [Authorize(Policy = PolicyNames.RequireMarketStaff)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLevyPayments(string caretakerId, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _caretakerService.GetLevyPayments(caretakerId, paginationFilter);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpGet("levy-payments/{levyId}")]
    [Authorize(Policy = PolicyNames.RequireMarketStaff)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyLevyPaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyLevyPaymentResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLevyPaymentDetails(string levyId)
    {
        var response = await _caretakerService.GetLevyPaymentDetails(levyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }
    [HttpPost("create-goodboys")]
    [Authorize(Policy = PolicyNames.RequireCaretakerOnly)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddGoodBoy(string caretakerId, [FromBody] CreateGoodBoyDto request)
    {
        var response = await _caretakerService.CreateGoodBoy(caretakerId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpPut("update-goodboys/{goodBoyId}")]
    [Authorize(Policy = PolicyNames.RequireCaretakerOnly)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<GoodBoyResponseDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGoodBoy(string goodBoyId, [FromBody] UpdateGoodBoyRequestDto request)
    {
        var response = await _caretakerService.UpdateGoodBoy(goodBoyId, request);
        return !response.IsSuccessful ? BadRequest(response) : Ok(response);
    }

    [HttpGet("{caretakerId}/goodboys")]
    [Authorize(Policy = PolicyNames.RequireMarketStaff)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGoodBoys(string caretakerId, [FromQuery] PaginationFilter paginationFilter)
    {
        var response = await _caretakerService.GetGoodBoys(caretakerId, paginationFilter);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPatch("{caretakerId}/goodboys/{goodBoyId}/unblock")]
    [Authorize(Policy = PolicyNames.RequireCaretakerOnly)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnblockGoodBoy(string caretakerId, string goodBoyId)
    {
        var response = await _caretakerService.UnblockGoodBoy(caretakerId, goodBoyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpPatch("{caretakerId}/goodboys/{goodBoyId}/block")]
    [Authorize(Policy = PolicyNames.RequireCaretakerOnly)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BlockGoodBoy(string caretakerId, string goodBoyId)
    {
        var response = await _caretakerService.BlockGoodBoy(caretakerId, goodBoyId);
        return !response.IsSuccessful ? NotFound(response) : Ok(response);
    }

    [HttpDelete("deletegoodboy{goodBoyId}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteGoodBoy(string goodBoyId)
    {
        var response = await _caretakerService.DeleteGoodBoyByCaretaker(goodBoyId);
        if (!response.IsSuccessful)
        {
            return StatusCode(response.Error.StatusCode, response);
        }
        return Ok(response);
    }
}