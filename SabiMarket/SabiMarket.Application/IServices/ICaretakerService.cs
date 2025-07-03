using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.MarketParticipants;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.IServices
{
    public interface ICaretakerService
    {

        // Caretaker Management
        Task<BaseResponse<CaretakerResponseDto>> GetCaretakerById(string userId);
        Task<BaseResponse<CaretakerResponseDto>> CreateCaretaker(CaretakerForCreationRequestDto caretakerDto);
        Task<BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>> GetCaretakers(PaginationFilter paginationFilter);

        // Trader Management
        Task<BaseResponse<bool>> AssignTraderToCaretaker(string caretakerId, string traderId);
        Task<BaseResponse<bool>> RemoveTraderFromCaretaker(string caretakerId, string traderId);
        Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetAssignedTraders(string caretakerId, PaginationFilter paginationFilter);

        // Levy Management
        Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>>> GetLevyPayments(string caretakerId, PaginationFilter paginationFilter);
        Task<BaseResponse<GoodBoyLevyPaymentResponseDto>> GetLevyPaymentDetails(string levyId);

        // GoodBoy Management
        //Task<BaseResponse<GoodBoyResponseDto>> AddGoodBoy(string caretakerId, CreateGoodBoyDto goodBoyDto);
        Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(string caretakerId, CreateGoodBoyDto request);
        Task<BaseResponse<GoodBoyResponseDto>> UpdateGoodBoy(string goodBoyId, UpdateGoodBoyRequestDto request);
        Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(string caretakerId, PaginationFilter paginationFilter);
        Task<BaseResponse<bool>> BlockGoodBoy(string caretakerId, string goodBoyId);
        Task<BaseResponse<bool>> UnblockGoodBoy(string caretakerId, string goodBoyId);
        //Task<BaseResponse<bool>> DeleteCaretakerByChairman(string caretakerId);
        Task<BaseResponse<bool>> DeleteCaretaker(string caretakerId);
        Task<BaseResponse<bool>> DeleteGoodBoyByCaretaker(string goodBoyId);
        Task<BaseResponse<CaretakerResponseDto>> UpdateCaretaker(string caretakerId, UpdateCaretakerRequestDto request);
    }
}
