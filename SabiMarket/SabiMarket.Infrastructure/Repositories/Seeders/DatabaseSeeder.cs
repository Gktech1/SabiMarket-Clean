using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using SabiMarket.Infrastructure.Utilities;

public static class UserRoles
{
    public const string Admin = "ADMIN";
    public const string Vendor = "VENDOR";
    public const string Customer = "CUSTOMER";
    public const string Advertiser = "ADVERTISER";
    public const string Goodboy = "GOODBOY";
    public const string AssistOfficer = "ASSIST_OFFICER";
    public const string Chairman = "CHAIRMAN";
    public const string Caretaker = "CARETAKER";
    public const string Trader = "TRADER";
    public const string TeamMember = "TEAMMEMBER";

}
public class DatabaseSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DatabaseSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger,
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor = null)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentIpAddress()
    {
        return _httpContextAccessor.GetRemoteIPAddress();
    }
    public async Task SeedAsync()
    {
        try
         {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");

        // First ensure system user exists
        var systemUserId = await EnsureSystemUserExistsAsync();

        // Define base roles with their permissions based on UI checkboxes
        var roleDefinitions = new Dictionary<string, string[]>
    {
        {
            UserRoles.Admin,
            new[] {
                "ViewTraders", "ViewLevies", "ScanTradersQRCode",
                "AddLevies", "AddTraders"
            }
        },
        {
            UserRoles.Vendor,
            new[] {
                "ViewTraders",
            }
        },
        {
            UserRoles.Customer,
            new[] {
                "ViewTraders"
            }
        },
        {
            UserRoles.Advertiser,
            new[] {
                "ViewTraders", "ViewLevies"
            }
        },
        {
            UserRoles.AssistOfficer,
            new[] {
                "ViewTraders", "ViewLevies", "ScanTradersQRCode"
            }
        },
        {
            UserRoles.Goodboy,
            new[] {
                "ViewTraders"
            }
        },
        {
            UserRoles.Trader,
            new[] {
                "ViewTraders", "ViewLevies"
            }
        },
        {
            UserRoles.Chairman,
            new[] {
                "ViewTraders", "ViewLevies", "ScanTradersQRCode",
                "AddLevies", "AddTraders"
            }
        },
        {
            UserRoles.TeamMember,
            new[] {
                "ViewTraders", 
            }
        }
    };

        foreach (var roleDef in roleDefinitions)
        {
            var roleName = roleDef.Key;
            var permissions = roleDef.Value;

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new ApplicationRole(roleName)
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Name = roleName,
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "System",
                    Description = $"Role for {roleName}",
                    IsActive = true,
                    Permissions = permissions.Select(p => new RolePermission
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = p,
                        IsGranted = true,
                        RoleId = roleName
                    }).ToList()
                };

                var result = await _roleManager.CreateAsync(role);


                if (result.Succeeded)
                {
                    var auditLog = new AuditLog
                    {
                        UserId = systemUserId,  // Use the system user ID
                        Activity = $"Role created: {roleName}",
                        Module = "Role Management",
                        Details = $"Role {roleName} created with {permissions.Length} permissions",
                        IpAddress = GetCurrentIpAddress()
                    };
                    auditLog.SetDateTime(DateTime.UtcNow);

                    _context.AuditLogs.Add(auditLog);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}. Errors: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Failed to create role {roleName}");
                }
            }
            else
            {
                _logger.LogInformation("Role already exists: {RoleName}", roleName);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        _logger.LogInformation("Seeding admin user...");
        var adminEmail = "admin@yourapp.com";
        var adminPassword = "YourSecurePassword123!";
        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin credentials are not configured properly.");
        }
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                NormalizedEmail = adminEmail.ToUpper(),
                NormalizedUserName = adminEmail.ToUpper(),
                Address = "Default Admin Address",
                ProfileImageUrl = "default-admin-avatar.png",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PhoneNumber = "+1234567890",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                LastLoginAt = null
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, UserRoles.Admin);

                // Create Admin entity with dashboard access and stats
                var admin = new Admin
                {
                    UserId = adminUser.Id,
                    Position = "System Administrator",
                    Department = "IT Administration",
                    AdminLevel = "Super Admin",
                    HasDashboardAccess = true,
                    HasRoleManagementAccess = true,
                    HasTeamManagementAccess = true,
                    HasAuditLogAccess = true,
                    RegisteredLGAs = 0,
                    ActiveChairmen = 0,
                    TotalRevenue = 0,
                    StatsLastUpdatedAt = DateTime.UtcNow
                };

                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();

                // Add initial audit log
                var auditLog = new AuditLog
                {
                    UserId = adminUser.Id,
                    Activity = "Admin user created",
                    Module = "Authentication",
                    Details = "Initial admin user created during system setup",
                    IpAddress = GetCurrentIpAddress()
                };
                auditLog.SetDateTime(DateTime.UtcNow);

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user. Errors: {errors}");
            }
        }
    }

    private async Task<string> EnsureSystemUserExistsAsync()
    {
        const string SYSTEM_EMAIL = "system@sabimarket.com";

        var systemUser = await _userManager.FindByEmailAsync(SYSTEM_EMAIL);
        if (systemUser == null)
        {
            systemUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = SYSTEM_EMAIL,
                Email = SYSTEM_EMAIL,
                FirstName = "System",
                LastName = "Account",
                EmailConfirmed = true,
                NormalizedEmail = SYSTEM_EMAIL,
                NormalizedUserName = SYSTEM_EMAIL,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Address = "Default Admin Address",
                ProfileImageUrl = "default-admin-avatar.png",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PhoneNumber = "+2234567890",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                LastLoginAt = null
            };

            var result = await _userManager.CreateAsync(systemUser, Guid.NewGuid().ToString());
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to create system user");
            }
        }

        return systemUser.Id;
    }
}


/* private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");
        string[] roles = {
        UserRoles.Admin,
        UserRoles.Vendor,
        UserRoles.Customer,
        UserRoles.Advertiser,
        UserRoles.AssistOfficer,
        UserRoles.Goodboy,
        UserRoles.Trader,
        UserRoles.Chairman
    };
        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole(roleName)  // Changed from IdentityRole
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = roleName,   
                    Name = roleName,
                    LastModifiedAt = DateTime.UtcNow,   
                    Permissions = "ALL",
                    Description = $"This is {roleName}",
                    IsActive = true   
                });
                _logger.LogInformation("Created role: {RoleName}", roleName);
            }
        }
    }*/
