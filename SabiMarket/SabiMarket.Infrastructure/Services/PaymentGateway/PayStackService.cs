using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayStack.Net;
using SabiMarket.Application.DTOs.PaymentsDto;
using SabiMarket.Infrastructure.Data;

namespace SabiMarket.Infrastructure.Services;

public class PayStackService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly PayStackApi _payStack;
    ApplicationDbContext _context;
    public string Url { get; set; }

    public PayStackService(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _secretKey = _configuration["Payment:PayStackSecretKey"];
        _payStack = new PayStackApi(_secretKey);
        _context = context;
    }

    public async Task<Tuple<bool, string, string>> InitializePayment(FundWalletVM model)
    {
        var senderEmail = await _context.Users.Where(x => x.Id == model.UserId)
            .Select(s => s.Email)
            .FirstOrDefaultAsync();

        var transactionRef = $"SabiMart_" + Guid.NewGuid();

        var request = new TransactionInitializeRequest
        {
            AmountInKobo = (int)model.Amount * 100,
            Email = senderEmail ?? "",
            Currency = "NGN",
            CallbackUrl = _configuration["Payment:PayStackCallbackUrl"],
            Reference = transactionRef,
        };

        var response = _payStack.Transactions.Initialize(request);

        if (!response.Status) return new Tuple<bool, string, string>(false, response.Message, transactionRef);

        return new Tuple<bool, string, string>(true, response.Data.AuthorizationUrl, transactionRef);
    }

    public async Task<bool> Verify(string reference)
    {
        var verifyResponse = _payStack.Transactions.Verify(reference);

        return verifyResponse.Status;
    }

    public async Task<IEnumerable<SabiMarket.Application.DTOs.PaymentsDto.Bank>> GetListOfBanks()
    {
        var result = _payStack.Get<ApiResponse<dynamic>>("bank?currency=NGN");

        if (!result.Status)
            throw new Exception("Unable to fetch banks");

        var banks = result.Data.ToObject<List<SabiMarket.Application.DTOs.PaymentsDto.Bank>>();

        return banks;
    }


}