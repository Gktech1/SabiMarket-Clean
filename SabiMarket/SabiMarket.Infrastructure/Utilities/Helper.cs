using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SabiMarket.Infrastructure.Utilities
{
    public static class Helper
    {
        public static (string Id, string Role, string Email) GetUserDetails(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext.User;
            var Id = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value?.ToLower();
            var role = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value?.ToLower();
            var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value?.ToLower();

            return (Id, role, email);
        }
    }
}

