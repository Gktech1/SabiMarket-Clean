using SabiMarket.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Infrastructure.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static UserClaimsDto GetUserClaims(this ClaimsPrincipal principal)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
                return null;

            return new UserClaimsDto
            {
                UserId = principal.FindFirstValue(ClaimTypes.NameIdentifier),
                Email = principal.FindFirstValue(ClaimTypes.Email),
                Username = principal.FindFirstValue(ClaimTypes.Name),
                FirstName = principal.FindFirstValue(ClaimTypes.GivenName),
                LastName = principal.FindFirstValue(ClaimTypes.Surname),
                Role = principal.FindFirstValue(ClaimTypes.Role),
                ProfileImageUrl = principal.FindFirstValue("profile_image"),
                LastLoginAt = DateTime.Parse(principal.FindFirstValue("last_login") ?? DateTime.UtcNow.ToString("O")),
                AdditionalDetails = GetAdditionalDetailsByRole(principal)
            };
        }

        private static IDictionary<string, object> GetAdditionalDetailsByRole(ClaimsPrincipal principal)
        {
            var role = principal.FindFirstValue(ClaimTypes.Role);
            var details = new Dictionary<string, object>();

            switch (role?.ToUpper())
            {
                case UserRoles.Admin:
                    details.Add("adminId", principal.FindFirstValue("admin_id"));
                    details.Add("permissions", principal.FindFirstValue("permissions"));
                    break;

                case UserRoles.Vendor:
                    details.Add("vendorId", principal.FindFirstValue("vendor_id"));
                    details.Add("marketId", principal.FindFirstValue("market_id"));
                    details.Add("businessName", principal.FindFirstValue("business_name"));
                    break;

                case UserRoles.Customer:
                    details.Add("customerId", principal.FindFirstValue("customer_id"));
                    details.Add("customerType", principal.FindFirstValue("customer_type"));
                    break;

                case UserRoles.Advertiser:
                    details.Add("advertiserId", principal.FindFirstValue("advertiser_id"));
                    details.Add("companyName", principal.FindFirstValue("company_name"));
                    break;

                case UserRoles.Goodboy:
                    details.Add("goodBoyId", principal.FindFirstValue("goodboy_id"));
                    details.Add("caretakerId", principal.FindFirstValue("caretaker_id"));
                    details.Add("marketId", principal.FindFirstValue("market_id"));
                    break;

                case UserRoles.AssistOfficer:
                    details.Add("officerId", principal.FindFirstValue("officer_id"));
                    details.Add("department", principal.FindFirstValue("department"));
                    details.Add("marketId", principal.FindFirstValue("market_id"));
                    break;

                case UserRoles.Chairman:
                    details.Add("chairmanId", principal.FindFirstValue("chairman_id"));
                    details.Add("marketId", principal.FindFirstValue("market_id"));
                    details.Add("tenure", principal.FindFirstValue("tenure"));
                    break;

                case UserRoles.Caretaker:
                    details.Add("caretakerId", principal.FindFirstValue("caretaker_id"));
                    details.Add("marketId", principal.FindFirstValue("market_id"));
                    details.Add("zone", principal.FindFirstValue("zone"));
                    break;

                case UserRoles.Trader:
                    details.Add("traderId", principal.FindFirstValue("trader_id"));
                    details.Add("caretakerId", principal.FindFirstValue("caretaker_id"));
                    details.Add("marketId", principal.FindFirstValue("market_id"));
                    details.Add("shopNumber", principal.FindFirstValue("shop_number"));
                    break;
            }

            // Add common details that might be useful across roles
            details.Add("isActive", principal.FindFirstValue("is_active"));
            details.Add("lastLoginAt", principal.FindFirstValue("last_login"));
            details.Add("createdAt", principal.FindFirstValue("created_at"));

            return details;
        }
    }
}
