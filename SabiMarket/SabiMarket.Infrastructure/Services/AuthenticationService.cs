using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace SabiMarket.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IValidator<RegistrationRequestDto> _registrationValidator;
        private readonly IValidator<LoginRequestDto> _loginValidator;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IRepositoryManager _repositoryMannager;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger,
            IValidator<RegistrationRequestDto> registrationValidator,
            RoleManager<ApplicationRole> roleManager,
            IValidator<LoginRequestDto> loginValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IRepositoryManager repositoryMannager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _registrationValidator = registrationValidator;
            _roleManager = roleManager;
            _loginValidator = loginValidator;
            _changePasswordValidator = changePasswordValidator;
            _repositoryMannager = repositoryMannager;
        }

        public async Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                // Validate request using FluentValidation
                var validationResult = await _loginValidator.ValidateAsync(loginRequest);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Find user by email
                //var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                var user = await _userManager.Users
                                        .Include(u => u.Admin)
                                        .Include(u => u.Chairman)
                                        .Include(u => u.Trader)
                                        .Include(u => u.Vendor)
                                        .Include(u => u.Customer)
                                        .Include(u => u.GoodBoy)
                                        .Include(u => u.Caretaker)
                                        .Include(u => u.AssistCenterOfficer)
                                        .Include(u => u.LocalGovernment)
                                        .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
                if (user == null)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new NotFoundException("Invalid email or password"),
                        "User not found");
                }

                if (!user.IsActive)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new ForbidException("Account is deactivated"),
                        "Account inactive");
                }

                // Verify password and sign in
                var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, false, true);
                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new BadRequestException("Invalid email or password"),
                        "Login failed");
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any())
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new UnauthorizedException("No roles assigned to this user"),
                        "No roles assigned");
                }

                // Get associated entity based on role
                var userDetails = await GetUserDetailsByRole(user, roles.First());

                // Generate JWT token with claims
                //var (token, expiresAt) = await GenerateJwtTokenAsync(user, roles.First(), userDetails);
                var (token, expiresAt, jwtId) = await GenerateJwtTokenAsync(user, roles.First(), userDetails);

                // Generate and save refresh token with all properties

                user.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                user.RefreshTokenJwtId = jwtId;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30); // Set appropriate expiry
                user.IsRefreshTokenUsed = false;
                user.LastLoginAt = DateTime.UtcNow;

                // Save the updated user with refresh token
                await _userManager.UpdateAsync(user);

                // Generate refresh token
                // user.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                var response = new LoginResponseDto
                {
                    AccessToken = token,
                    RefreshToken = user.RefreshToken,
                    ExpiresAt = expiresAt,
                    UserInfo = new UserClaimsDto
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Username = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = roles.First(),
                        LastLoginAt = user.LastLoginAt ?? DateTime.UtcNow,
                        ProfileImageUrl = user.ProfileImageUrl,
                        AdditionalDetails = userDetails,
                    }
                };

                return ResponseFactory.Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for email: {Email}", loginRequest.Email);
                return ResponseFactory.Fail<LoginResponseDto>(
                    new BadRequestException("An unexpected error occurred during login"),
                    "Login failed");
            }
        }


        public async Task<BaseResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Find user with valid refresh token
                /*var user = await _userManager.Users.FirstOrDefaultAsync(u =>
                    u.RefreshToken == refreshToken &&
                    u.RefreshTokenExpiryTime > DateTime.UtcNow &&
                    u.IsRefreshTokenUsed != true);*/

                var user = await _userManager.Users.FirstOrDefaultAsync(u =>
                                        u.RefreshToken == refreshToken &&
                                        u.RefreshTokenExpiryTime > DateTime.UtcNow &&
                                        (u.IsRefreshTokenUsed == null || u.IsRefreshTokenUsed == false));

                if (user == null)
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new BadRequestException("Invalid or expired refresh token"),
                        "Invalid token");
                }

                // Mark current refresh token as used
                user.IsRefreshTokenUsed = true;
                await _userManager.UpdateAsync(user);

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any())
                {
                    return ResponseFactory.Fail<LoginResponseDto>(
                        new UnauthorizedException("No roles assigned to this user"),
                        "No roles assigned");
                }

                var userDetails = await GetUserDetailsByRole(user, roles.First());

                // Generate new tokens
                var (token, expiresAt, jwtId) = await GenerateJwtTokenAsync(user, roles.First(), userDetails);

                // Generate new refresh token
                user.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                user.RefreshTokenJwtId = jwtId;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
                user.IsRefreshTokenUsed = false;
                await _userManager.UpdateAsync(user);

                var response = new LoginResponseDto
                {
                    AccessToken = token,
                    RefreshToken = user.RefreshToken,
                    ExpiresAt = expiresAt,
                    UserInfo = new UserClaimsDto
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        Username = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = roles.First(),
                        LastLoginAt = user.LastLoginAt ?? DateTime.UtcNow,
                        ProfileImageUrl = user.ProfileImageUrl,
                        AdditionalDetails = userDetails
                    }
                };

                return ResponseFactory.Success(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while refreshing token");
                return ResponseFactory.Fail<LoginResponseDto>(
                    new BadRequestException("An unexpected error occurred while refreshing token"),
                    "Refresh failed");
            }
        }

        public async Task<BaseResponse<RegistrationResponseDto>> RegisterAsync(RegistrationRequestDto request)
        {
            try
            {
                // Validate email format
                var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(request.Email, emailPattern))
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException("Invalid email format"),
                        "Please provide a valid email address");
                }

                // Validate password strength
                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException("Password must be at least 8 characters long"),
                        "Invalid password");
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException("Email already registered"),
                        "Email exists");
                }

                // Validate role
                if (!await _roleManager.RoleExistsAsync(request.Role.ToUpper()))
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException("Invalid role specified"),
                        "Invalid role");
                }

                // Create base user
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    ProfileImageUrl = request.ProfileImageUrl ?? "",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Create user with role
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description))),
                        "User creation failed");
                }

                // Handle role-specific logic
                switch (request.Role.ToUpper())
                {
                    case "VENDOR":
                        var vendor = new Vendor
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = user.Id,
                            LocalGovernmentId = request.VendorDetails.LocalGovernmentId.ToString() ?? "",
                            BusinessName = request.VendorDetails.BusinessName,
                            BusinessAddress = request.Address,
                            BusinessDescription = request.VendorDetails.BusinessDescription,
                            VendorCode = $"V-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                            Type = request.VendorDetails.VendorTypeEnum ?? VendorTypeEnum.Other, // Replace with appropriate default,
                            IsVerified = false,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            VendorCurrencyType = request.VendorDetails.VendorCurrencyTypeEnum.Value
                        };

                        _repositoryMannager.VendorRepository.Create(vendor);
                        break;

                    case "CUSTOMER":
                        var customer = new Customer
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = user.Id,
                            LocalGovernmentId = request.CustomerDetails.LocalGovernmentId.ToString() ?? "",
                            FullName = $"{request.FirstName} {request.LastName}",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _repositoryMannager.CustomerRepository.CreateCustomer(customer);
                        break;

                    case "ADVERTISER":

                        var advertisement = new Advertisement
                        {
                            Id = Guid.NewGuid().ToString(),
                            VendorId = user.Id,  // Using the user ID since they're the advertiser
                            Title = request.AdvertiserDetails.CompanyName,
                            Description = request.AdvertiserDetails.BusinessType,
                            Status = AdvertStatusEnum.Pending,
                            Language = "en", // Default language, could be made configurable
                            Location = request.Address,
                            AdvertPlacement = "Default", // This could be made configurable
                            PaymentStatus = "Pending",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _repositoryMannager.AdvertisementRepository.CreateAdvertisement(advertisement);
                        break;

                    default:
                        await _userManager.DeleteAsync(user);
                        return ResponseFactory.Fail<RegistrationResponseDto>(
                            new BadRequestException($"Registration for role {request.Role} is not supported"),
                            "Unsupported role type");
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, request.Role.ToUpper());
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    return ResponseFactory.Fail<RegistrationResponseDto>(
                        new BadRequestException("Failed to assign role"),
                        "Role assignment failed");
                }

                var response = new RegistrationResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = request.Role,
                    Message = "Registration successful. You can now log in",
                    RequiresApproval = request.Role.ToUpper() is "VENDOR" or "ADVERTISER"
                };

                return ResponseFactory.Success(response, "Registration successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
                return ResponseFactory.Fail<RegistrationResponseDto>(
                    new BadRequestException("An unexpected error occurred during registration"),
                    "Registration failed");
            }
        }

        private bool RequiresApproval(string role)
        {
            return role.ToUpper() switch
            {
                UserRoles.Vendor => true,
                UserRoles.Trader => true,
                UserRoles.Chairman => true,
                UserRoles.Caretaker => true,
                UserRoles.Goodboy => true,
                UserRoles.Customer => true,
                _ => false
            };
        }

        /* private string GetRegistrationMessage(string role)
         {
            // var requiresVerification = RequiresEmailVerification(role);
             var requiresApproval = RequiresApproval(role);

             return (requiresApproval) switch
             {
                 (true) => "Registration successful. Please verify your email and wait for admin approval.",
                 (true, false) => "Registration successful. Please verify your email to activate your account.",
                 (false, true) => "Registration successful. Your account is pending admin approval.",
                 _ => "Registration successful. You can now log in."
             };
         }*/



        private async Task<IDictionary<string, object>> GetUserDetailsByRole(ApplicationUser user, string role)
        {
            var details = new Dictionary<string, object>();

            switch (role.ToUpper())
            {
                case "ADMIN":
                    details.Add("admin", user.AdminId);
                    details.Add("address", user.Address);
                    break;

                case "CHAIRMAN":
                    if (user.Chairman != null)
                    {
                        details.Add("chairmanId", user.Chairman.Id);
                        details.Add("title", user.Chairman.Title);
                        details.Add("office", user.Chairman.Office);
                        details.Add("termStart", user.Chairman.TermStart);
                        details.Add("termEnd", user.Chairman.TermEnd);
                        details.Add("localGovernmentId", user.Chairman.LocalGovernmentId);
                    }
                    break;

                case "TRADER":
                    if (user.Trader != null)
                    {
                        details.Add("traderId", user.Trader.Id);
                        details.Add("traderbusinessName", user.Trader.BusinessName);
                        details.Add("traderbusinessType", user.Trader.TIN);
                        details.Add("businessType", user.Trader.BusinessType);
                        details.Add("traderMarketId", user.Trader.MarketId);
                        details.Add("traderCaretakerId", user.Trader.CaretakerId);
                        details.Add("traderMarketName", user.Trader.MarketName);
                        details.Add("traderPaymentFrequency", user.Trader.PaymentFrequency);

                    }
                    break;

                case "VENDOR":
                    if (user.Vendor != null)
                    {
                        details.Add("vendorId", user.Vendor.Id);
                        details.Add("businessName", user.Vendor.BusinessName);
                        details.Add("businessDescription", user.Vendor.BusinessDescription);
                        details.Add("businessAddress", user.Vendor.BusinessAddress);
                        details.Add("vendorLocalGovernmentId", user.Vendor.LocalGovernmentId);
                        details.Add("vendorIsSubscriptionActive", user.Vendor.IsSubscriptionActive);
                        details.Add("VendorAdvertisements", user.Vendor.Advertisements);
                    }
                    break;

                case "CUSTOMER":
                    if (user.Customer != null)
                    {
                        details.Add("customerId", user.Customer.Id);
                        details.Add("fullname", user.Customer.FullName);
                    }
                    break;

                case "GOODBOY":
                    if (user.GoodBoy != null)
                    {
                        //details.Add("goodBoyTraderId", user.Trader.Id);
                        details.Add("goodBoyId", user.GoodBoy.Id);
                        details.Add("Status", user.GoodBoy.Status);
                        details.Add("GoodBoyCaretakerId", user.GoodBoy?.CaretakerId);
                        details.Add("goodBoyIdMarketId", user.GoodBoy.MarketId);

                    }
                    break;

                case "CARETAKER":
                    if (user.Caretaker != null)
                    {
                        details.Add("caretaker", new
                        {
                            Id = user.Caretaker.Id,
                            MarketId = user.Caretaker.MarketId,
                            ChairmanId = user.Caretaker.ChairmanId,
                            LocalGovernmentId = user.Caretaker.LocalGovernmentId,
                            IsBlocked = user.Caretaker.IsBlocked
                        });
                    }
                    break;

                case "ASSIST_OFFICER":
                    if (user.AssistCenterOfficer != null)
                    {
                        details.Add("officerId", user.AssistCenterOfficer.Id);
                        details.Add("assistCenterOfficerChairmanId", user.AssistCenterOfficer.ChairmanId);
                        details.Add("assistCenterOfficerLocalGovernmentId", user.AssistCenterOfficer.LocalGovernmentId);
                        details.Add("assistCenterOfficerMarketId", user.AssistCenterOfficer?.MarketId);
                        details.Add("assistCenterOfficerIsActive", user.AssistCenterOfficer.IsActive);
                        details.Add("assistCenterOfficerIsBlocked", user.AssistCenterOfficer.IsBlocked);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown role encountered: {Role}", role);
                    break;
            }

            //if (user.LocalGovernmentId.HasValue)
            //{
            //    details.Add("localGovernmentId", user.LocalGovernmentId.Value);
            //}
            /* if (user.LocalGovernmentId != null)
             {
                 details.Add("localGovernmentId", user.LocalGovernmentId);
             }*/
            if (user.LocalGovernmentId != null && !details.ContainsKey("localGovernmentId"))
            {
                details.Add("localGovernmentId", user.LocalGovernmentId);
            }
            return details;
        }

        private async Task<(string token, DateTime expiresAt, string jwtId)> GenerateJwtTokenAsync(
    ApplicationUser user,
    string role,
    IDictionary<string, object> additionalDetails)
        {
            try
            {
                // Generate JWT ID
                var jwtId = Guid.NewGuid().ToString();  // Add this line at the beginning

                // Validate JWT configuration
                var jwtSecret = _configuration["JwtSettings:Secret"];
                var validIssuer = _configuration["JwtSettings:ValidIssuer"];
                var validAudience = _configuration["JwtSettings:ValidAudience"];
                if (string.IsNullOrEmpty(jwtSecret))
                {
                    _logger.LogError("JWT Secret is not configured");
                    throw new InvalidOperationException("JWT configuration is missing");
                }
                if (string.IsNullOrEmpty(validIssuer) || string.IsNullOrEmpty(validAudience))
                {
                    _logger.LogError("JWT Issuer or Audience is not configured");
                    throw new InvalidOperationException("JWT configuration is incomplete");
                }
                // Validate user data
                if (user == null || string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.Email))
                {
                    throw new ArgumentException("Invalid user data");
                }
                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, jwtId),  // Add the JWT ID claim
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                    new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
                    new Claim(ClaimTypes.Role, role ?? string.Empty),
                    new Claim("profile_image", user.ProfileImageUrl ?? string.Empty),
                    new Claim("last_login", user.LastLoginAt?.ToString("O") ?? DateTime.UtcNow.ToString("O"))
                };

                // Add additional role-specific claims with null checking
                if (additionalDetails != null)
                {
                    foreach (var detail in additionalDetails)
                    {
                        if (!string.IsNullOrEmpty(detail.Key))
                        {
                            claims.Add(new Claim(detail.Key, detail.Value?.ToString() ?? string.Empty));
                        }
                    }
                }

                // Create token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiresAt = DateTime.UtcNow.AddMinutes(30);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = expiresAt,
                    SigningCredentials = creds,
                    Issuer = validIssuer,
                    Audience = validAudience
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return (tokenHandler.WriteToken(token), expiresAt, jwtId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user: {UserId}", user?.Id);
                throw;
            }
        }

    }
}

