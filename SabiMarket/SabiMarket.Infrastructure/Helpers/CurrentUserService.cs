using Microsoft.AspNetCore.Http;
using SabiMarket.Application.DTOs.Responses;
using System.Security.Claims;

namespace SabiMarket.Infrastructure.Helpers
{
    public interface ICurrentUserService
    {
        UserClaimsDto GetCurrentUser();
        string GetUserId();
        string GetUserRole();
        bool IsInRole(string role);
        IDictionary<string, object> GetUserDetails();
        T GetDetailValue<T>(string key);
        string GetMarketId();
        string GetVendorId();
        string GetCaretakerId();
        string GetChairmanId();
        string GetTraderId();
        string GetGoodBoyId();
        bool IsAuthenticated();
        bool IsAdmin();
        bool IsVendor();
        bool IsCustomer();
        bool IsAdvertiser();
        bool IsGoodBoy();
        bool IsAssistOfficer();
        bool IsChairman();
        bool IsCaretaker();
        bool IsTrader();
        bool IsMarketStaff();
        bool IsMarketManagement();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserClaimsDto _currentUser;
        private readonly IDictionary<string, object> _userDetails;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _currentUser = _httpContextAccessor.HttpContext?.User?.GetUserClaims();
            _userDetails = _currentUser?.AdditionalDetails ?? new Dictionary<string, object>();
        }

        public UserClaimsDto GetCurrentUser() => _currentUser;

        public string GetUserId() =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string GetUserRole() =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

        public bool IsAuthenticated() =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role) =>
            _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

        public IDictionary<string, object> GetUserDetails() => _userDetails;

        public T GetDetailValue<T>(string key)
        {
            if (_userDetails.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return default;
                }
            }
            return default;
        }

        // Entity-specific ID getters
        public string GetMarketId() => GetDetailValue<string>("marketId");
        public string GetVendorId() => GetDetailValue<string>("vendorId");
        public string GetCaretakerId() => GetDetailValue<string>("caretakerId");
        public string GetChairmanId() => GetDetailValue<string>("chairmanId");
        public string GetTraderId() => GetDetailValue<string>("traderId");
        public string GetGoodBoyId() => GetDetailValue<string>("goodBoyId");

        // Role-specific checkers
        public bool IsAdmin() => IsInRole(UserRoles.Admin);
        public bool IsVendor() => IsInRole(UserRoles.Vendor);
        public bool IsCustomer() => IsInRole(UserRoles.Customer);
        public bool IsAdvertiser() => IsInRole(UserRoles.Advertiser);
        public bool IsGoodBoy() => IsInRole(UserRoles.Goodboy);
        public bool IsAssistOfficer() => IsInRole(UserRoles.AssistOfficer);
        public bool IsChairman() => IsInRole(UserRoles.Chairman);
        public bool IsCaretaker() => IsInRole(UserRoles.Caretaker);
        public bool IsTrader() => IsInRole(UserRoles.Trader);

        // Combined role checks
        public bool IsMarketStaff() =>
            IsCaretaker() || IsGoodBoy() || IsAssistOfficer();

        public bool IsMarketManagement() =>
            IsAdmin() || IsChairman() || IsCaretaker();
    }
}