using Microsoft.Extensions.Configuration;

namespace SabiMarket.Infrastructure.Configuration
{
    public class BankPaymentSettings
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
    }

    public interface IBankSettingsService
    {
        BankPaymentSettings GetBankSettings();
    }

    public class BankSettingsService : IBankSettingsService
    {
        private readonly IConfiguration _configuration;

        public BankSettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BankPaymentSettings GetBankSettings()
        {
            return new BankPaymentSettings
            {
                BankName = _configuration.GetValue<string>("PaymentSettings:BankName"),
                AccountNumber = _configuration.GetValue<string>("PaymentSettings:AccountNumber"),
                AccountName = _configuration.GetValue<string>("PaymentSettings:AccountName")
            };
        }
    }
}