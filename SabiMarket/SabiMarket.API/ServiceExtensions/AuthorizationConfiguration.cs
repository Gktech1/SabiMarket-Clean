using Microsoft.AspNetCore.Authorization;

namespace SabiMarket.API.ServiceExtensions
{
    public static class PolicyNames
    {
        public const string RequireAdminOnly = "RequireAdminOnly";
        public const string RequireVendorOnly = "RequireVendorOnly";
        public const string RequireCustomerOnly = "RequireCustomerOnly";
        public const string RequireCaretakerOnly = "RequireCaretakerOnly";
        public const string RequireTraderOnly = "RequireTraderOnly";
        public const string RequiredTeamMember = "RequireTeamMember";
        public const string RequireAssistOfficer = "RequireAssistOfficer";



        // Combined policies
        public const string RequireMarketStaff = "RequireMarketStaff";
        public const string RequireMarketManagement = "RequireMarketManagement";
        public const string RequiredVendorCustomerAndAdmin = "RequiredVendorCustomerAndAdmin ";
    }

    public static class AuthorizationConfiguration
    {
        public static void ConfigureAuthorization(AuthorizationOptions options)
        {
            // Single role policies
            options.AddPolicy(PolicyNames.RequireAdminOnly,
                policy => policy.RequireRole(UserRoles.Admin));

            options.AddPolicy(PolicyNames.RequireVendorOnly,
                policy => policy.RequireRole(UserRoles.Vendor));

            options.AddPolicy(PolicyNames.RequireCustomerOnly,
                policy => policy.RequireRole(UserRoles.Customer));

            options.AddPolicy(PolicyNames.RequireCaretakerOnly,
                policy => policy.RequireRole(UserRoles.Caretaker));

            options.AddPolicy(PolicyNames.RequireTraderOnly,
                policy => policy.RequireRole(UserRoles.Trader));

            options.AddPolicy(PolicyNames.RequireAssistOfficer,
              policy => policy.RequireRole( UserRoles.AssistOfficer));

            // Combined role policies
            options.AddPolicy(PolicyNames.RequireMarketStaff,
                policy => policy.RequireRole(
                    UserRoles.Caretaker,
                    UserRoles.Goodboy,
                    UserRoles.AssistOfficer
                ));

            // Combined role policies
            options.AddPolicy(PolicyNames.RequiredVendorCustomerAndAdmin,
                policy => policy.RequireRole(
                    UserRoles.Customer,
                    UserRoles.Vendor,
                    UserRoles.Admin
                    )); 

            options.AddPolicy(PolicyNames.RequireMarketManagement,
                policy => policy.RequireRole(
                    UserRoles.Admin,
                    UserRoles.Chairman,
                    UserRoles.Caretaker,
                    UserRoles.AssistOfficer,
                    UserRoles.Trader
                ));
        }
    }
}
