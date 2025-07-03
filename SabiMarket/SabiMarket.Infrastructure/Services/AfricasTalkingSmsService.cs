using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SabiMarket.Infrastructure.Services
{
    public interface ISmsService
    {
        Task<bool> SendSMS(string phoneNumber, string message);
    }

    public class AfricasTalkingSmsService : ISmsService
    {
        private readonly string _username;
        private readonly string _apiKey;
        private readonly string _fromNumber; // Sender ID
        private readonly string _apiUrl; // API URL
        private readonly ILogger<AfricasTalkingSmsService> _logger;
        private readonly HttpClient _httpClient;

        public AfricasTalkingSmsService(IConfiguration configuration, ILogger<AfricasTalkingSmsService> logger, HttpClient httpClient)
        {
            _username = configuration["AfricasTalking:Username"];
            _apiKey = configuration["AfricasTalking:ApiKey"];
            _fromNumber = configuration["AfricasTalking:SenderId"]; // Custom sender ID
            _apiUrl = configuration["AfricasTalking:ApiUrl"]; // Read API URL from config
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<bool> SendSMS(string phoneNumber, string message)
        {
            try
            {
                // Convert local number to international format if necessary
                string formattedPhoneNumber = FormatPhoneNumber(phoneNumber);

                var requestBody = new
                {
                    username = _username,
                    to = formattedPhoneNumber,
                    message = message,
                    from = _fromNumber
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Add("apiKey", _apiKey);
                var response = await _httpClient.PostAsync(_apiUrl, jsonContent); // Use API URL from config

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"SMS sent successfully to {formattedPhoneNumber}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to send SMS: {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS");
                return false;
            }
        }

        /// <summary>
        /// Converts a local Nigerian number (e.g., 08169765343) to international format (+2348169765343).
        /// </summary>
        private string FormatPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 11)
            {
                return "+234" + phoneNumber.Substring(1); // Remove '0' and add +234
            }
            return phoneNumber; // If already in correct format, return as is
        }
    }
}
































/*using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SabiMarket.Infrastructure.Services
{
    public interface ISmsService
    {
        Task<bool> SendSMS(string phoneNumber, string message);
    }

    public class TwilioSmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;
        private readonly ILogger<TwilioSmsService> _logger;

        public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _fromNumber = configuration["Twilio:FromNumber"];
            _logger = logger;
        }

        public async Task<bool> SendSMS(string phoneNumber, string message)
        {
            try
            {
                // Convert local number to international format if necessary
                string formattedPhoneNumber = FormatPhoneNumber(phoneNumber);

                TwilioClient.Init(_accountSid, _authToken);

                var messageResource = await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(_fromNumber),
                    to: new Twilio.Types.PhoneNumber(formattedPhoneNumber)
                );

                _logger.LogInformation($"SMS sent. SID: {messageResource.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS");
                return false;
            }
        }

        /// <summary>
        /// Converts a local Nigerian number (e.g., 08169765343) to international format (+2348169765343).
        /// </summary>
        private string FormatPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.StartsWith("0") && phoneNumber.Length == 11)
            {
                return "+234" + phoneNumber.Substring(1); // Remove '0' and add +234
            }
            return phoneNumber; // If already in correct format, return as is
        }

    }
}
*/