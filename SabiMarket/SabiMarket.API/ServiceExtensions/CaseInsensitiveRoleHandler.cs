using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class CaseInsensitiveRoleHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            if (requirement is RolesAuthorizationRequirement rolesRequirement)
            {
                if (context.User == null || rolesRequirement.AllowedRoles == null)
                {
                    continue;
                }

                var userRoles = context.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value);

                foreach (var role in rolesRequirement.AllowedRoles)
                {
                    if (userRoles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Succeed(requirement);
                        break;
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}