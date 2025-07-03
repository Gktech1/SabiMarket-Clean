using SabiMarket.Application.DTOs.PaymentsDto;
using SabiMarket.Application.DTOs.Responses;


public interface IPayments
{
    public Task<BaseResponse<string>> Initialize(FundWalletVM model);
    public Task<BaseResponse<bool>> Verify(string reference);
    public Task<IEnumerable<Bank>> GetBanks();
}