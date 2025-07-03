using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Infrastructure.Utilities;
using System.Text.Json;
using SabiMarket.Application.Interfaces;
using ValidationException = FluentValidation.ValidationException;
using SabiMarket.Services.Dtos.Levy;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using SabiMarket.Domain.Entities.MarketParticipants;

namespace SabiMarket.Infrastructure.Services
{
    public class GoodBoysService : IGoodBoysService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<GoodBoysService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<CreateGoodBoyRequestDto> _createGoodBoyValidator;
        private readonly IConfiguration _configuration;
        //private readonly IValidator<UpdateGoodBoyProfileDto> _updateProfileValidator;

        public GoodBoysService(
            IRepositoryManager repository,
            ILogger<GoodBoysService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IHttpContextAccessor httpContextAccessor,
            IValidator<CreateGoodBoyRequestDto> createGoodBoyValidator, IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _createGoodBoyValidator = createGoodBoyValidator;
            _configuration = configuration;
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }

        private async Task CreateAuditLog(string activity, string details, string module = "GoodBoys Management")
        {
            var userId = _currentUser.GetUserId();
            var auditLog = new AuditLog
            {
                UserId = userId,
                Activity = activity,
                Module = module,
                Details = details,
                IpAddress = GetCurrentIpAddress()
            };
            auditLog.SetDateTime(DateTime.UtcNow);

            _repository.AuditLogRepository.Create(auditLog);
            await _repository.SaveChangesAsync();
        }

        public async Task<BaseResponse<GoodBoyResponseDto>> GetGoodBoyById(string goodBoyId)
        {
            try
            {
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(goodBoyId, trackChanges: false);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "GoodBoy Lookup Failed",
                        $"Failed to find GoodBoy with ID: {goodBoyId}",
                        "GoodBoy Query"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                var goodBoyDto = _mapper.Map<GoodBoyResponseDto>(goodBoy);

                await CreateAuditLog(
                    "GoodBoy Lookup",
                    $"Retrieved GoodBoy details for ID: {goodBoyId}",
                    "GoodBoy Query"
                );

                return ResponseFactory.Success(goodBoyDto, "GoodBoy retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GoodBoy");
                return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(CreateGoodBoyRequestDto goodBoyDto)
        {
            try
            {
                var validationResult = await _createGoodBoyValidator.ValidateAsync(goodBoyDto);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "GoodBoy Creation Failed",
                        $"Validation failed for new GoodBoy creation with email: {goodBoyDto.Email}",
                        "GoodBoy Creation"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        "Validation failed");
                }

                var existingUser = await _userManager.FindByEmailAsync(goodBoyDto.Email);
                if (existingUser != null)
                {
                    await CreateAuditLog(
                        "GoodBoy Creation Failed",
                        $"Email already exists: {goodBoyDto.Email}",
                        "GoodBoy Creation"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>("Email already exists");
                }

                // Validate CaretakerId exists
                var caretakerExists = await _repository.CaretakerRepository.GetCaretakerById(goodBoyDto.CaretakerId, false);
                if (caretakerExists == null)
                {
                    await CreateAuditLog(
                        "GoodBoy Creation Failed",
                        $"Invalid CaretakerId: {goodBoyDto.CaretakerId} does not exist",
                        "GoodBoy Creation"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>("Invalid CaretakerId provided");
                }

                // Validate MarketId exists (if you have a Market repository)
                var marketExists = await _repository.MarketRepository.GetMarketById(goodBoyDto.MarketId, false);
                if (marketExists == null)
                {
                    await CreateAuditLog(
                        "GoodBoy Creation Failed",
                        $"Invalid MarketId: {goodBoyDto.MarketId} does not exist",
                        "GoodBoy Creation"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>("Invalid MarketId provided");
                }

                var fullname = $"{goodBoyDto.FirstName}  {goodBoyDto.LastName}";
                // Manual mapping instead of AutoMapper to ensure proper initialization
                var defaultPassword = GenerateDefaultPassword(fullname);
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(), // Explicitly set the Id
                    UserName = goodBoyDto.Email,
                    Email = goodBoyDto.Email,
                    PhoneNumber = goodBoyDto.PhoneNumber,
                    FirstName = goodBoyDto.FirstName,
                    LastName = goodBoyDto.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsBlocked = false,
                    EmailConfirmed = true,
                    Gender = goodBoyDto.Gender ?? string.Empty,
                    ProfileImageUrl = goodBoyDto.ProfileImage ?? string.Empty,
                    LocalGovernmentId = goodBoyDto.LocalGovernmentId ?? string.Empty
                };

                var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "GoodBoy Creation Failed",
                        $"Failed to create user account for: {goodBoyDto.Email}. Errors: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}",
                        "GoodBoy Creation"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        $"Failed to create user: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                }

                var goodBoy = new GoodBoy
                {
                    UserId = user.Id,
                    CaretakerId = goodBoyDto.CaretakerId,
                    MarketId = goodBoyDto.MarketId,
                    Status = StatusEnum.Blocked
                };

                _repository.GoodBoyRepository.AddGoodBoy(goodBoy);
                await _repository.SaveChangesAsync();

                await _userManager.AddToRoleAsync(user, UserRoles.Goodboy);

                await CreateAuditLog(
                    "Created GoodBoy Account",
                    $"Created GoodBoy account for {user.Email} ({user.FirstName} {user.LastName})",
                    "GoodBoy Creation"
                );

                var createdGoodBoy = _mapper.Map<GoodBoyResponseDto>(goodBoy);
                createdGoodBoy.DefaultPassword = defaultPassword;
                return ResponseFactory.Success(createdGoodBoy, "GoodBoy created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GoodBoy");
                return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
            }
        }
        private string GenerateDefaultPassword(string fullName)
        {
            var nameParts = fullName.Split(' '); // Split the full name into first name and last name
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : ""; // Handle cases where only one name is provided

            var random = new Random();
            var randomNumbers = random.Next(100, 999).ToString(); // Generate a 3-digit random number

            // Special characters pool
            var specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
            var specialChar1 = specialChars[random.Next(specialChars.Length)];
            var specialChar2 = specialChars[random.Next(specialChars.Length)];

            // Generate random uppercase letters
            var uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var uppercaseLetter = uppercaseLetters[random.Next(uppercaseLetters.Length)];

            // Combine first name, last name, random number, and special characters
            // Make sure at least one character in the name parts is uppercase
            var firstNameProcessed = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower();
            var lastNameProcessed = !string.IsNullOrEmpty(lastName)
                ? char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower()
                : "";

            // Combine all elements with special characters and uppercase
            var password = $"{firstNameProcessed}{specialChar1}{lastNameProcessed}{randomNumbers}{uppercaseLetter}{specialChar2}";

            // Ensure password has minimum complexity
            if (password.Length < 8)
            {
                // Add additional random characters for very short names
                var additionalChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
                while (password.Length < 8)
                {
                    password += additionalChars[random.Next(additionalChars.Length)];
                }
            }

            return password;
        }


        /*  public async Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(CreateGoodBoyRequestDto goodBoyDto)
          {
              try
              {
                  var validationResult = await _createGoodBoyValidator.ValidateAsync(goodBoyDto);
                  if (!validationResult.IsValid)
                  {
                      await CreateAuditLog(
                          "GoodBoy Creation Failed",
                          $"Validation failed for new GoodBoy creation with email: {goodBoyDto.Email}",
                          "GoodBoy Creation"
                      );
                      return ResponseFactory.Fail<GoodBoyResponseDto>(
                          "Validation failed");
                  }

                  var existingUser = await _userManager.FindByEmailAsync(goodBoyDto.Email);
                  if (existingUser != null)
                  {
                      await CreateAuditLog(
                          "GoodBoy Creation Failed",
                          $"Email already exists: {goodBoyDto.Email}",
                          "GoodBoy Creation"
                      );
                      return ResponseFactory.Fail<GoodBoyResponseDto>("Email already exists");
                  }

                  // Manual mapping instead of AutoMapper to ensure proper initialization
                  var user = new ApplicationUser
                  {
                      Id = Guid.NewGuid().ToString(), // Explicitly set the Id
                      UserName = goodBoyDto.Email,
                      Email = goodBoyDto.Email,
                      PhoneNumber = goodBoyDto.PhoneNumber,
                      FirstName = goodBoyDto.FirstName,
                      LastName = goodBoyDto.LastName,
                      CreatedAt = DateTime.UtcNow,
                      IsActive = true,
                      IsBlocked = false,
                      EmailConfirmed = true,
                      Gender = goodBoyDto.Gender ?? string.Empty,
                      ProfileImageUrl = goodBoyDto.ProfileImage,
                      LocalGovernmentId = goodBoyDto.LocalGovernmentId
                  };
                  var createUserResult = await _userManager.CreateAsync(user);
                  if (!createUserResult.Succeeded)
                  {
                      await CreateAuditLog(
                          "GoodBoy Creation Failed",
                          $"Failed to create user account for: {goodBoyDto.Email}. Errors: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}",
                          "GoodBoy Creation"
                      );
                      return ResponseFactory.Fail<GoodBoyResponseDto>(
                          $"Failed to create user: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                  }

                  var goodBoy = new GoodBoy
                  {
                      UserId = user.Id,
                      CaretakerId = goodBoyDto.CaretakerId,
                      MarketId = goodBoyDto.MarketId,
                      Status = StatusEnum.Unlocked
                  };

                  _repository.GoodBoyRepository.AddGoodBoy(goodBoy);
                  await _repository.SaveChangesAsync();

                  await _userManager.AddToRoleAsync(user, UserRoles.Goodboy);

                  await CreateAuditLog(
                      "Created GoodBoy Account",
                      $"Created GoodBoy account for {user.Email} ({user.FirstName} {user.LastName})",
                      "GoodBoy Creation"
                  );

                  var createdGoodBoy = _mapper.Map<GoodBoyResponseDto>(goodBoy);
                  return ResponseFactory.Success(createdGoodBoy, "GoodBoy created successfully");
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error creating GoodBoy");
                  return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
              }
          }*/

        /* public async Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(CreateGoodBoyRequestDto goodBoyDto)
         {
             try
             {
                 var validationResult = await _createGoodBoyValidator.ValidateAsync(goodBoyDto);
                 if (!validationResult.IsValid)
                 {
                     await CreateAuditLog(
                         "GoodBoy Creation Failed",
                         $"Validation failed for new GoodBoy creation with email: {goodBoyDto.Email}",
                         "GoodBoy Creation"
                     );
                     return ResponseFactory.Fail<GoodBoyResponseDto>(
                         "Validation failed");
                 }

                 var existingUser = await _userManager.FindByEmailAsync(goodBoyDto.Email);
                 if (existingUser != null)
                 {
                     await CreateAuditLog(
                         "GoodBoy Creation Failed",
                         $"Email already exists: {goodBoyDto.Email}",
                         "GoodBoy Creation"
                     );
                     return ResponseFactory.Fail<GoodBoyResponseDto>("Email already exists");
                 }

                 var user = _mapper.Map<ApplicationUser>(goodBoyDto);

                 var createUserResult = await _userManager.CreateAsync(user);
                 if (!createUserResult.Succeeded)
                 {
                     await CreateAuditLog(
                         "GoodBoy Creation Failed",
                         $"Failed to create user account for: {goodBoyDto.Email}",
                         "GoodBoy Creation"
                     );
                     return ResponseFactory.Fail<GoodBoyResponseDto>(
                         "Failed to create user");
                 }

                 var goodBoy = new GoodBoy
                 {
                     UserId = user.Id,
                     CaretakerId = goodBoyDto.CaretakerId,
                     MarketId = goodBoyDto.MarketId,
                     Status = StatusEnum.Blocked
                 };

                 _repository.GoodBoyRepository.AddGoodBoy(goodBoy);
                 await _repository.SaveChangesAsync();

                 await _userManager.AddToRoleAsync(user, UserRoles.Goodboy);

                 await CreateAuditLog(
                     "Created GoodBoy Account",
                     $"Created GoodBoy account for {user.Email} ({user.FirstName} {user.LastName})"
                 );

                 var createdGoodBoy = _mapper.Map<GoodBoyResponseDto>(goodBoy);
                 return ResponseFactory.Success(createdGoodBoy, "GoodBoy created successfully");
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error creating GoodBoy");
                 return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
             }
         }
 */
        public async Task<BaseResponse<bool>> UpdateGoodBoyProfile(string goodBoyId, UpdateGoodBoyProfileDto profileDto)
        {
            try
            {
               /* var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"Validation failed for GoodBoy profile update ID: {goodBoyId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }*/

                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(goodBoyId, trackChanges: true);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"GoodBoy not found for ID: {goodBoyId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                var user = await _userManager.FindByIdAsync(goodBoy.UserId);
                if (user == null)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"User not found for GoodBoy ID: {goodBoyId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "User not found");
                }

                // Track changes for audit log
                var changes = new List<string>();
                if (user.PhoneNumber != profileDto.PhoneNumber)
                    changes.Add($"Phone: {user.PhoneNumber} → {profileDto.PhoneNumber}");

                // Update user properties
                user.PhoneNumber = profileDto.PhoneNumber;

                var updateUserResult = await _userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Profile Update Failed",
                        $"Failed to update user properties for GoodBoy ID: {goodBoyId}",
                        "Profile Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        "Failed to update user");
                }

                _repository.GoodBoyRepository.UpdateGoodBoy(goodBoy);

                await CreateAuditLog(
                    "Updated GoodBoy Profile",
                    $"Updated profile for {user.Email}. Changes: {string.Join(", ", changes)}",
                    "Profile Management"
                );

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "GoodBoy profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GoodBoy profile");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(
            GoodBoyFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                var query = _repository.GoodBoyRepository.FindAll(false);

                // Apply filters
                if (!string.IsNullOrEmpty(filterDto.MarketId))
                    query = query.Where(g => g.MarketId == filterDto.MarketId);

                if (!string.IsNullOrEmpty(filterDto.CaretakerId))
                    query = query.Where(g => g.CaretakerId == filterDto.CaretakerId);

                if (filterDto.Status.HasValue)
                    query = query.Where(g => g.Status == filterDto.Status.Value);

                var paginatedGoodBoys = await query.Paginate(paginationFilter);

                var goodBoyDtos = _mapper.Map<IEnumerable<GoodBoyResponseDto>>(paginatedGoodBoys.PageItems);
                var result = new PaginatorDto<IEnumerable<GoodBoyResponseDto>>
                {
                    PageItems = goodBoyDtos,
                    PageSize = paginatedGoodBoys.PageSize,
                    CurrentPage = paginatedGoodBoys.CurrentPage,
                    NumberOfPages = paginatedGoodBoys.NumberOfPages
                };

                await CreateAuditLog(
                    "GoodBoy List Query",
                    $"Retrieved GoodBoy list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                    "GoodBoy Query"
                );

                return ResponseFactory.Success(result, "GoodBoys retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GoodBoys");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> ProcessLevyPayment(string goodBoyId, ProcessLevyPaymentDto paymentDto)
        {
            try
            {
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(goodBoyId, trackChanges: false);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "Levy Payment Failed",
                        $"GoodBoy not found for ID: {goodBoyId}",
                        "Levy Payment"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                var levyPayment = _mapper.Map<LevyPayment>(paymentDto);
                levyPayment.GoodBoyId = goodBoyId;

                _repository.LevyPaymentRepository.Create(levyPayment);

                await CreateAuditLog(
                    "Levy Payment Processed",
                    $"Processed levy payment of {paymentDto.Amount} for GoodBoy ID: {goodBoyId}",
                    "Levy Payment"
                );

                await _repository.SaveChangesAsync();
                return ResponseFactory.Success(true, "Levy payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing levy payment");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<TraderDetailsDto>> GetTraderDetails(string traderId)
        {
            try
            {
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(traderId, trackChanges: false);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "Trader Details Lookup Failed",
                        $"Failed to find trader with ID: {traderId}",
                        "Trader Query"
                    );
                    return ResponseFactory.Fail<TraderDetailsDto>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                var traderDetails = _mapper.Map<TraderDetailsDto>(goodBoy);
                traderDetails.ProfileImageUrl = goodBoy.User.ProfileImageUrl;

                await CreateAuditLog(
                    "Trader Details Lookup",
                    $"Retrieved trader details for ID: {traderId}",
                    "Trader Query"
                );

                return ResponseFactory.Success(traderDetails, "Trader details retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trader details");
                return ResponseFactory.Fail<TraderDetailsDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<TraderQRValidationResponseDto>> ValidateTraderQRCode(ScanTraderQRCodeDto scanDto)
        {
            try
            {
                // Validate QR code format (OSH/LAG/23401)
                /*if (!scanDto.QRCodeData.StartsWith("OSH/LAG/"))
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        "Invalid trader QR code");
                }*/

                if(string.IsNullOrEmpty(scanDto?.TraderId))
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                       "traderId is required");
                }

                //var traderId = scanDto.QRCodeData.Replace("OSH/LAG/", "");

                // Get the trader by ID
                var trader = await _repository.TraderRepository.GetTraderById(scanDto?.TraderId, trackChanges: false);

                if (trader == null)
                {
                    await CreateAuditLog(
                        "QR Code Validation Failed",
                        $"Invalid trader ID from QR Code: {scanDto?.TraderId}",
                        "Payment Processing"
                    );
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        new NotFoundException("Trader not found"),
                        "Invalid trader QR code");
                }

                // Check if scanning user is authorized (must be a GoodBoy)
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyByUserId(scanDto.ScannedByUserId);
                if (goodBoy == null)
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        new UnauthorizedException("Unauthorized scan attempt"),
                        "Unauthorized to scan trader QR codes");
                }
                var levySetup = await _repository.LevyPaymentRepository
                   .GetActiveLevySetupByMarketAndOccupancyAsync(trader.MarketId, trader.TraderOccupancy);

                if (levySetup == null)
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        new BadRequestException("Levy setup not configured"),
                        "Levy setup not configured for this market and occupancy type");
                }

                // Get market details
                var marketDetail = await _repository.MarketRepository
                    .GetMarketByIdAsync(trader.MarketId, false);

                if (marketDetail == null)
                {
                    return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                        new UnauthorizedException("Market not found"),
                        "Market not found");
                }
                // Get trader's actual payment history (excluding setup records)
                var paymentHistory = await _repository.LevyPaymentRepository
                    .GetTraderPaymentHistory(trader.Id, excludeSetupRecords: true);

                // Calculate levy breakdown based on occupancy type and payment history
                var levyBreakdown = await CalculateLevyBreakdown(trader, levySetup, paymentHistory);

                // Get the most recent actual payment (using correct enum values)
                var lastPayment = paymentHistory
                    .Where(p => p.PaymentStatus == PaymentStatusEnum.Paid)
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault();

                // Count unique building/occupancy types for this trader
                var buildingTypesCount = await GetTraderBuildingTypesCountWithBusinessLogic(trader.Id);

                // Format payment frequency
                var paymentFrequency = FormatPaymentFrequency(levySetup.Period, levySetup.Amount);

                // Build update payment URL
                var updatePaymentUrl = _configuration.GetSection("ProcessPaymentUrl").Value;
                var fullUpdateUrl = $"{updatePaymentUrl}api/GoodBoys/updatetraderpayment/{trader.Id}";

                // Create response
                var validationResponse = new TraderQRValidationResponseDto
                {
                    TraderId = trader.Id,
                    TraderName = $"{trader.User.FirstName} {trader.User.LastName}",
                    TraderOccupancy = FormatOccupancyType(trader.TraderOccupancy),
                    TraderIdentityNumber = trader.TIN,
                    PaymentFrequency = paymentFrequency,
                    TotalAmount = levyBreakdown.TotalAmount,
                    MarketId = trader.MarketId,
                    MarketName = marketDetail.MarketName,
                    PaymentPeriod = levySetup.Period,
                    LastPaymentDate = lastPayment?.PaymentDate,
                    UpdatePaymentUrl = fullUpdateUrl,
                    NumberOfBuildingTypes = buildingTypesCount,
                    LevyBreakdown = levyBreakdown,
                    BusinessName = trader.BusinessName,
                    OccupancyType = trader.TraderOccupancy,
                    ProfileImageUrl = trader.User.ProfileImageUrl
                };

                // Get payment frequency and amount from most recent levy payment for this market and trader occupancy
                /* var levySetups = await _repository.LevyPaymentRepository.GetByMarketAndOccupancyAsync(
                     trader.MarketId,
                     trader.TraderOccupancy);

                 string paymentFrequency = "Not configured";
                 LevyPayment latestSetup = null;
                 if (levySetups != null && levySetups.Any())
                 {
                     // Get the most recent levy payment setup for this trader occupancy
                         latestSetup = levySetups
                         .OrderByDescending(lp => lp.CreatedAt)
                         .FirstOrDefault();

                     if (latestSetup != null)
                     {
                         // Format payment frequency string based on period and amount
                         paymentFrequency = $"{GetPeriodDays(latestSetup.Period)} days - N{latestSetup.Amount}";
                     }
                 }

                 // Get the most recent payment for this trader
                 var latestPayment = trader.LevyPayments
                     .OrderByDescending(p => p.PaymentDate)
                     .FirstOrDefault();

                 var updatepaymenturl = _configuration.GetSection("ProcesspaymentUrl").Value;
                //https://localhost:7111/api/GoodBoys/updatetraderpayment/8FFF4B79-DA26-4628-A3F2-4CFFBC07DAC9

                 // Create response with dynamic data from the trader entity
                 var validationResponse = new TraderQRValidationResponseDto
                 {
                     TraderId = trader.Id,
                     TraderName = $"{trader.User.FirstName} {trader.User.LastName}",
                     TraderOccupancy = trader.TraderOccupancy.ToString(),
                     TraderIdentityNumber =   trader.TIN, //$"OSH/LAG/{trader.Id}",
                     PaymentFrequency = paymentFrequency,
                     TotalAmount = latestSetup.Amount,
                     PaymentPeriod = latestSetup.Period,
                     LastPaymentDate = latestPayment?.PaymentDate,
                     UpdatePaymentUrl = $"{updatepaymenturl}api/GoodBoys/updatetraderpayment/{scanDto?.TraderId}"
                 };*/

                await CreateAuditLog(
                    "Trader QR Code Scanned",
                    $"Trader QR Code scanned by GoodBoy: {goodBoy.Id} for Trader: {trader.Id}",
                    "Payment Processing"
                );

                return ResponseFactory.Success(validationResponse, "Trader QR code validated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating trader QR code");
                return ResponseFactory.Fail<TraderQRValidationResponseDto>(ex, "An unexpected error occurred");
            }
        }

        private async Task<int> GetTraderBuildingTypesCountWithBusinessLogic(string traderId)
        {
            if (string.IsNullOrEmpty(traderId))
                return 0;

            // Get the actual count from database
            var count = await _repository.TraderRepository.GetDistinctTraderBuildingTypesCount(traderId);

            // If no building types found, you might want to check the trader's primary occupancy
            if (count == 0)
            {
                var trader = await _repository.TraderRepository.GetTraderById(traderId, false);
                if (trader != null)
                {
                    // If trader exists but has no specific building types, 
                    // you could return 1 based on their primary occupancy type
                    switch (trader.TraderOccupancy)
                    {
                        case MarketTypeEnum.Shop:
                        case MarketTypeEnum.Kiosk:
                        case MarketTypeEnum.OpenSpace:
                        case MarketTypeEnum.WareHouse:
                            return 1; // Default to 1 if trader exists but no building types recorded
                        default:
                            return 0;
                    }
                }
            }

            return count;
        }


        private string FormatPaymentFrequency(PaymentPeriodEnum period, decimal amount)
        {
            var days = (int)period; // Since your enum values are the actual days
            return $"{days} days - ₦{amount:N0}";
        }

        private string FormatOccupancyType(MarketTypeEnum occupancyType)
        {
            return occupancyType switch
            {
                MarketTypeEnum.OpenSpace => "Open Space",
                MarketTypeEnum.Kiosk => "Kiosk",
                MarketTypeEnum.Shop => "Shop",
                MarketTypeEnum.WareHouse => "WareHouse",
                _ => occupancyType.ToString()
            };
        }
        private async Task<LevyBreakdownDto> CalculateLevyBreakdown(
            Trader trader,
            LevyPayment levySetup,
            IEnumerable<LevyPayment> paymentHistory)
        {
            var breakdown = new LevyBreakdownDto();

            // Calculate based on trader's occupancy type
            switch (trader.TraderOccupancy)
            {
                case MarketTypeEnum.OpenSpace:
                    breakdown.CurrentOpenSpaceLevy = levySetup.Amount;
                    breakdown.TotalUnpaidOpenSpaceLevy = await CalculateUnpaidAmount(
                        trader.Id, MarketTypeEnum.OpenSpace, paymentHistory);
                    break;

                case MarketTypeEnum.Kiosk:
                    breakdown.CurrentKioskLevy = levySetup.Amount;
                    breakdown.TotalUnpaidKioskLevy = await CalculateUnpaidAmount(
                        trader.Id, MarketTypeEnum.Kiosk, paymentHistory);
                    break;

                case MarketTypeEnum.Shop:
                    breakdown.CurrentShopLevy = levySetup.Amount;
                    breakdown.TotalUnpaidShopLevy = await CalculateUnpaidAmount(
                        trader.Id, MarketTypeEnum.Shop, paymentHistory);
                    break;

                case MarketTypeEnum.WareHouse:
                    breakdown.CurrentWareHouseLevy = levySetup.Amount;
                    breakdown.TotalUnpaidwareHouseLevy = await CalculateUnpaidAmount(
                        trader.Id, MarketTypeEnum.WareHouse, paymentHistory);
                    break;
            }

            breakdown.TotalAmount = breakdown.CurrentOpenSpaceLevy + breakdown.CurrentKioskLevy +
                                    breakdown.CurrentShopLevy + breakdown.CurrentWareHouseLevy +
                                    breakdown.TotalUnpaidOpenSpaceLevy + breakdown.TotalUnpaidKioskLevy +
                                    breakdown.TotalUnpaidShopLevy + breakdown.TotalUnpaidwareHouseLevy;

            // Calculate overdue days
            breakdown.OverdueDays = await CalculateOverdueDays(trader.Id, paymentHistory);
            breakdown.PaymentStatus = GetPaymentStatus(paymentHistory);

            return breakdown;
        }

        private async Task<decimal> CalculateUnpaidAmount(
            string traderId,
            MarketTypeEnum occupancyType,
            IEnumerable<LevyPayment> paymentHistory)
        {
            // Get all pending/unpaid/failed payments for this occupancy type
            var unpaidPayments = paymentHistory
                .Where(p => !p.IsSetupRecord && // Exclude setup records
                           (p.PaymentStatus == PaymentStatusEnum.Pending ||
                            p.PaymentStatus == PaymentStatusEnum.Unpaid ||
                            p.PaymentStatus == PaymentStatusEnum.Failed))
                .ToList();

            // Calculate total unpaid amount
            return unpaidPayments.Sum(p => p.Amount);
        }

        private async Task<int> CalculateOverdueDays(string traderId, IEnumerable<LevyPayment> paymentHistory)
        {
            var oldestUnpaidPayment = paymentHistory
                .Where(p => !p.IsSetupRecord &&
                           (p.PaymentStatus == PaymentStatusEnum.Pending ||
                            p.PaymentStatus == PaymentStatusEnum.Unpaid))
                .OrderBy(p => p.DueDate ?? p.PaymentDate)
                .FirstOrDefault();

            if (oldestUnpaidPayment?.DueDate != null)
            {
                var daysDiff = (DateTime.Now - oldestUnpaidPayment.DueDate.Value).Days;
                return daysDiff > 0 ? daysDiff : 0;
            }

            return 0;
        }

        private string GetPaymentStatus(IEnumerable<LevyPayment> paymentHistory)
        {
            var hasUnpaid = paymentHistory.Any(p => !p.IsSetupRecord &&
                                                   (p.PaymentStatus == PaymentStatusEnum.Pending ||
                                                    p.PaymentStatus == PaymentStatusEnum.Unpaid));

            var hasOverdue = paymentHistory.Any(p => !p.IsSetupRecord &&
                                                    p.DueDate.HasValue &&
                                                    p.DueDate < DateTime.Now &&
                                                    p.PaymentStatus != PaymentStatusEnum.Paid);

            if (hasOverdue) return "Overdue";
            if (hasUnpaid) return "Pending";
            return "Up to Date";
        }


        // Helper method to convert PaymentPeriodEnum to number of days
        private int GetPeriodDays(PaymentPeriodEnum period)
        {
            return period switch
            {
                PaymentPeriodEnum.Daily => 1,
                PaymentPeriodEnum.Weekly => 7,
                PaymentPeriodEnum.BiWeekly => 14,
                PaymentPeriodEnum.Monthly => 30,
                PaymentPeriodEnum.Quarterly => 90,
                PaymentPeriodEnum.HalfYearly => 180,
                PaymentPeriodEnum.Yearly => 365,
                _ => 0
            };
        }

        /* public async Task<BaseResponse<TraderQRValidationResponseDto>> ValidateTraderQRCode(ScanTraderQRCodeDto scanDto)
         {
             try
             {
                 // Validate QR code format (OSH/LAG/23401)
                 if (!scanDto.QRCodeData.StartsWith("OSH/LAG/"))
                 {
                     return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                         "Invalid trader QR code");
                 }

                 var traderId = scanDto.QRCodeData.Replace("OSH/LAG/", "");
                 var trader = await _repository.GoodBoyRepository.GetGoodBoyById(traderId, trackChanges: false);

                 if (trader == null)
                 {
                     await CreateAuditLog(
                         "QR Code Validation Failed",
                         $"Invalid trader ID from QR Code: {traderId}",
                         "Payment Processing"
                     );
                     return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                         new NotFoundException("Trader not found"),
                         "Invalid trader QR code");
                 }

                 // Check if scanning user is authorized
                 var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyByUserId(scanDto.ScannedByUserId);
                 if (goodBoy == null)
                 {
                     return ResponseFactory.Fail<TraderQRValidationResponseDto>(
                         new UnauthorizedException("Unauthorized scan attempt"),
                         "Unauthorized to scan trader QR codes");
                 }

                 var validationResponse = new TraderQRValidationResponseDto
                 {
                     TraderId = trader.Id,
                     TraderName = $"{trader.User.FirstName} {trader.User.LastName}",
                     TraderOccupancy = "Open Space",
                     TraderIdentityNumber = $"OSH/LAG/{trader.Id}",
                     PaymentFrequency = "2 days - N500",
                     LastPaymentDate = trader.LevyPayments
                         .OrderByDescending(p => p.PaymentDate)
                         .FirstOrDefault()?.PaymentDate,
                     UpdatePaymentUrl = ""
                 };

                 await CreateAuditLog(
                     "Trader QR Code Scanned",
                     $"Trader QR Code scanned by GoodBoy: {goodBoy.Id} for Trader: {trader.Id}",
                     "Payment Processing"
                 );

                 return ResponseFactory.Success(validationResponse, "Trader QR code validated successfully");
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error validating trader QR code");
                 return ResponseFactory.Fail<TraderQRValidationResponseDto>(ex, "An unexpected error occurred");
             }
         }
 */
        public async Task<BaseResponse<bool>> VerifyTraderPaymentStatus(string traderId)
        {
            try
            {
                var trader = await _repository.GoodBoyRepository.GetGoodBoyById(traderId, trackChanges: false);
                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                var lastPayment = trader.LevyPayments
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault();

                if (lastPayment == null)
                {
                    return ResponseFactory.Success(false, "Payment required");
                }

                // Check if payment is within the 2-day window
                var daysSinceLastPayment = (DateTime.UtcNow - lastPayment.PaymentDate).TotalDays;
                var isPaymentValid = daysSinceLastPayment <= 2;

                return ResponseFactory.Success(isPaymentValid,
                    isPaymentValid ? "Payment is up to date" : "Payment required");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying trader payment status");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> ProcessTraderLevyPayment(string traderId, ProcessLevyPaymentDto paymentDto)
        {
            var userId = _currentUser.GetUserId();
            try
            {
                // 1. Get the trader
                var trader = await _repository.TraderRepository.GetByIdWithInclude(
                    traderId,
                    t => t.LevyPayments);

                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                // 2. Get the goodboy
                var goodboy = await _repository.GoodBoyRepository.GetGoodBoyById(paymentDto.GoodBoyId);
                if (goodboy == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Goodboy not found"),
                        "Goodboy not found");
                }

                // 3. Validate if goodboy can collect from this trader (same market and caretaker)
                if (trader.MarketId != goodboy.MarketId)
                {
                    return ResponseFactory.Fail<bool>(
                        new ValidationException("Goodboy is not authorized to collect payment from this trader"),
                        "Not authorized to collect payment from this trader (different market)");
                }

                // 4. Check if trader is under the goodboy's caretaker (if applicable)
                if (!string.IsNullOrEmpty(trader.CaretakerId) && trader.CaretakerId != goodboy.CaretakerId)
                {
                    return ResponseFactory.Fail<bool>(
                        new ValidationException("Goodboy is not authorized to collect payment from this trader"),
                        "Not authorized to collect payment from this trader (different caretaker)");
                }

                // 5. Get the last payment and check if payment is due based on frequency
                var lastPayment = trader.LevyPayments
                    .Where(p => p.PaymentStatus == PaymentStatusEnum.Paid)
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault();

                if (lastPayment != null)
                {
                    var paymentDue = IsPaymentDue(lastPayment, paymentDto.Period);
                    /*if (!paymentDue.isDue)
                    {
                        return ResponseFactory.Fail<bool>(
                            new ValidationException($"Payment not due yet. Next payment due on {paymentDue.nextPaymentDate}"),
                            $"Payment not due yet. Next payment due on {paymentDue.nextPaymentDate:dd/MM/yyyy}");
                    }*/
                }

                // 6. Create the levy payment
                var currentDate = DateTime.UtcNow;
                var levyPayment = new LevyPayment
                {
                    TraderId = traderId,
                    GoodBoyId = userId,
                    MarketId = trader.MarketId,
                    ChairmanId = trader.ChairmanId, // Get from market if needed
                    Amount = paymentDto.Amount,
                    Period = paymentDto.Period,
                    PaymentMethod = paymentDto.PaymentMethod ?? PaymenPeriodEnum.Cash,
                    PaymentStatus = PaymentStatusEnum.Paid,
                    TransactionReference = paymentDto.TransactionReference ?? GenerateTransactionReference(),
                    HasIncentive = paymentDto.HasIncentive,
                    IncentiveAmount = paymentDto.IncentiveAmount ?? 0,
                    PaymentDate = currentDate,
                    CollectionDate = currentDate,
                    Notes = paymentDto.Notes ?? "",
                    QRCodeScanned = paymentDto.QRCodeScanned,
                    IsSetupRecord = true,
                    IsActive = true,
                    OccupancyType = paymentDto.OccupancyType,
                };

                _repository.LevyPaymentRepository.Create(levyPayment);

                await CreateAuditLog(
                    "Levy Payment Processed",
                    $"Payment processed by GoodBoy: {goodboy.Id}  for Trader: {trader.TraderName}, Amount: ₦{paymentDto.Amount}",
                    "Payment Processing"
                );

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing trader levy payment for traderId: {TraderId}", traderId);
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred while processing payment");
            }
        }

        // Helper method to check if payment is due based on period
        private (bool isDue, DateTime nextPaymentDate) IsPaymentDue(LevyPayment lastPayment, PaymentPeriodEnum currentPeriod)
        {
            var lastPaymentDate = lastPayment.PaymentDate.Date;
            var currentDate = DateTime.UtcNow.Date;

            switch (lastPayment.Period)
            {
                case PaymentPeriodEnum.Daily:
                    var nextDailyPayment = lastPaymentDate.AddDays(1);
                    return (currentDate >= nextDailyPayment, nextDailyPayment);

                case PaymentPeriodEnum.Weekly:
                    var nextWeeklyPayment = lastPaymentDate.AddDays(7);
                    return (currentDate >= nextWeeklyPayment, nextWeeklyPayment);

                case PaymentPeriodEnum.BiWeekly:
                    var nextBiWeeklyPayment = lastPaymentDate.AddDays(14);
                    return (currentDate >= nextBiWeeklyPayment, nextBiWeeklyPayment);

                case PaymentPeriodEnum.Monthly:
                    var nextMonthlyPayment = lastPaymentDate.AddMonths(1);
                    return (currentDate >= nextMonthlyPayment, nextMonthlyPayment);

                case PaymentPeriodEnum.Quarterly:
                    var nextQuarterlyPayment = lastPaymentDate.AddMonths(3);
                    return (currentDate >= nextQuarterlyPayment, nextQuarterlyPayment);

                case PaymentPeriodEnum.Yearly:
                    var nextYearlyPayment = lastPaymentDate.AddYears(1);
                    return (currentDate >= nextYearlyPayment, nextYearlyPayment);

                default:
                    // If no specific period, allow payment (e.g., one-time payments)
                    return (true, currentDate);
            }
        }

        // Helper method to generate transaction reference
        private string GenerateTransactionReference()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }

        /*public async Task<BaseResponse<bool>> UpdateTraderPayment(string traderId, ProcessLevyPaymentDto paymentDto)
        {
            try
            {
                var trader = await _repository.GoodBoyRepository.GetGoodBoyById(traderId, trackChanges: false);
                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                // Verify payment hasn't already been made today
                var existingPayment = trader.LevyPayments
                    .Any(p => p.PaymentDate.Date == DateTime.UtcNow.Date);

                if (existingPayment)
                {
                    return ResponseFactory.Fail<bool>(
                        new ValidationException("Payment already processed for today"),
                        "Payment already processed for today");
                }

                var levyPayment = _mapper.Map<LevyPayment>(paymentDto);
                levyPayment.GoodBoyId = traderId;

                _repository.LevyPaymentRepository.Create(levyPayment);

                await CreateAuditLog(
                    "Levy Payment Updated",
                    $"Updated levy payment for Trader: {traderId}, Amount: {paymentDto.Amount}",
                    "Payment Processing"
                );

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trader payment");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }*/

        /*    public async Task<BaseResponse<IEnumerable<GoodBoyLevyPaymentResponseDto>>> GetTodayLeviesForGoodBoy(string goodBoyId)
            {
                var correlationId = Guid.NewGuid().ToString();
                var userId = _currentUser.GetUserId();
                try
                {
                    await CreateAuditLog(
                        "Today's Levies Query",
                        $"CorrelationId: {correlationId} - Retrieving today's levies for GoodBoy ID: {goodBoyId}",
                        "Levy Management"
                    );

                    var today = DateTime.Now.Date;
                    var tomorrow = today.AddDays(1);

                    var todayLevies = await _repository.LevyPaymentRepository.GetLevyPaymentsByDateRange(
                        goodBoyId);

                    var levyPaymentDtos = _mapper.Map<IEnumerable<GoodBoyLevyPaymentResponseDto>>(todayLevies);

                    await CreateAuditLog(
                        "Today's Levies Retrieved",
                        $"CorrelationId: {correlationId} - Retrieved {levyPaymentDtos.Count()} levies for GoodBoy ID: {goodBoyId}",
                        "Levy Management"
                    );

                    return ResponseFactory.Success(levyPaymentDtos, "Today's levies retrieved successfully");
                }
                catch (Exception ex)
                {
                    await CreateAuditLog(
                        "Today's Levies Query Failed",
                        $"CorrelationId: {correlationId} - Error: {ex.Message}",
                        "Levy Management"
                    );
                    return ResponseFactory.Fail<IEnumerable<GoodBoyLevyPaymentResponseDto>>(ex, "An unexpected error occurred");
                }
            }
    */

        /*   public async Task<BaseResponse<PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>>> GetTodayLeviesForGoodBoy(
        string goodBoyId,
        PaginationFilter pagination)
           {
               var correlationId = Guid.NewGuid().ToString();
               try
               {
                   await CreateAuditLog(
                       "Today's Levies Query",
                       $"CorrelationId: {correlationId} - Retrieving today's levies for GoodBoy ID: {goodBoyId}, Page {pagination.PageNumber}, Size {pagination.PageSize}",
                       "Levy Management"
                   );

                   // Get paginated levy payments from repository
                   var leviesPaginated = await _repository.LevyPaymentRepository.GetLevyPaymentsByDateRange(
                       goodBoyId,
                       pagination,
                       trackChanges: false
                   );

                   // Map entities to DTOs
                   var levyPaymentDtos = _mapper.Map<List<GoodBoyLevyPaymentResponseDto>>(leviesPaginated.PageItems);

                   // Manually fix the trader names after mapping
                   int index = 0;
                   foreach (var levy in leviesPaginated.PageItems)
                   {
                       if (levy.Trader?.User != null)
                       {
                           levyPaymentDtos[index].TraderName = $"{levy.Trader.User.FirstName} {levy.Trader.User.LastName}".Trim();
                       }
                       else
                       {
                           levyPaymentDtos[index].TraderName = "Unknown Trader";
                       }
                       levyPaymentDtos[index].Status = levy.PaymentStatus.ToString();
                       index++;
                   }

                   // Create final paginated result with DTOs
                   var paginatedResult = new PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>
                   {
                       PageItems = levyPaymentDtos,
                       CurrentPage = leviesPaginated.CurrentPage,
                       PageSize = leviesPaginated.PageSize,
                       NumberOfPages = leviesPaginated.NumberOfPages
                   };

                   await CreateAuditLog(
                       "Today's Levies Retrieved",
                       $"CorrelationId: {correlationId} - Retrieved {levyPaymentDtos.Count} levies for GoodBoy ID: {goodBoyId} on page {pagination.PageNumber}",
                       "Levy Management"
                   );

                   return ResponseFactory.Success(paginatedResult, "Today's levies retrieved successfully");
               }
               catch (Exception ex)
               {
                   await CreateAuditLog(
                       "Today's Levies Query Failed",
                       $"CorrelationId: {correlationId} - Error: {ex.Message}",
                       "Levy Management"
                   );
                   return ResponseFactory.Fail<PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>>(ex, "An unexpected error occurred");
               }
           }*/

        public async Task<BaseResponse<PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>>> GetTodayLeviesForGoodBoy(
    string goodBoyId,
    PaginationFilter pagination)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Today's Levies Query",
                    $"CorrelationId: {correlationId} - Retrieving today's levies for GoodBoy ID: {goodBoyId}, Page {pagination.PageNumber}, Size {pagination.PageSize}",
                    "Levy Management"
                );

                // Use the repository method that returns DTOs
                var leviesPaginated = await _repository.LevyPaymentRepository.GetLevyPaymentsByDateRange(
                    goodBoyId,
                    pagination,
                    trackChanges: false
                );

                // Convert from IEnumerable to List
                var levyPaymentsList = leviesPaginated.PageItems.ToList();

                // Create final paginated result
                var paginatedResult = new PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>
                {
                    PageItems = levyPaymentsList,
                    CurrentPage = leviesPaginated.CurrentPage,
                    PageSize = leviesPaginated.PageSize,
                    NumberOfPages = leviesPaginated.NumberOfPages
                };

                await CreateAuditLog(
                    "Today's Levies Retrieved",
                    $"CorrelationId: {correlationId} - Retrieved {levyPaymentsList.Count} levies for GoodBoy ID: {goodBoyId} on page {pagination.PageNumber}",
                    "Levy Management"
                );

                return ResponseFactory.Success(paginatedResult, "Today's levies retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Today's Levies Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<PaginatorDto<List<GoodBoyLevyPaymentResponseDto>>>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<GoodBoyDashboardStatsDto>> GetDashboardStats(
      string goodBoyId,
      DateTime? fromDate = null,
      DateTime? toDate = null,
      string searchQuery = null,
      PaginationFilter paginationFilter = null)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();
            try
            {
                await CreateAuditLog(
                    "Dashboard Stats Query",
                    $"CorrelationId: {correlationId} - Retrieving dashboard stats for GoodBoy ID: {goodBoyId}",
                    "Levy Management"
                );

                // Set default date range if not provided (current month)
                if (!fromDate.HasValue || !toDate.HasValue)
                {
                    fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                }

                // Set default pagination if not provided
                paginationFilter ??= new PaginationFilter();

                // Get trader count managed by the good boy
                var traderCount = await _repository.TraderRepository.GetTraderCountByGoodBoyIdAsync(goodBoyId);

                // Get total levy amount collected by the good boy in the date range
                var totalLevies = await _repository.LevyPaymentRepository.GetTotalLevyAmountByGoodBoyIdAsync(
                    userId,
                    fromDate.Value,
                    toDate.Value
                );

                // Get all levy payments for the good boy in the date range
                var allLevyPayments = await _repository.LevyPaymentRepository.GetLevyPaymentsByDateRangeAsync(
                    userId,
                    fromDate.Value,
                    toDate.Value
                );

                // Apply search filter if provided
                var filteredPayments = allLevyPayments;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.Trim().ToLower();
                    filteredPayments = allLevyPayments.Where(p =>
                        (p.TraderName != null && p.TraderName.ToLower().Contains(searchQuery))
                    // Add other search fields as needed, like transaction reference if available
                    );
                }

                // Apply pagination manually to filtered results
                var totalCount = filteredPayments.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / paginationFilter.PageSize);

                var paginatedPayments = filteredPayments
                    .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                    .Take(paginationFilter.PageSize)
                    .ToList();

                // Map DTOs to the required format
                var paymentDtos = paginatedPayments.Select(p => new LevyPaymentDto
                {
                    PayerName = p.TraderName ?? "Unknown Trader",
                    PaymentTime = p.PaymentDate,
                    Amount = p.Amount
                }).ToList();

                // Create final paginated result for payments
                var paginatedPaymentsResult = new PaginatorDto<List<LevyPaymentDto>>
                {
                    PageItems = paymentDtos,
                    CurrentPage = paginationFilter.PageNumber,
                    PageSize = paginationFilter.PageSize,
                    NumberOfPages = totalPages
                };

                // Convert PaginatorDto<List<T>> to PaginatorDto<IEnumerable<T>> for GoodBoyDashboardStatsDto
                var paginatedPaymentsForDto = new PaginatorDto<IEnumerable<LevyPaymentDto>>
                {
                    PageItems = paymentDtos,
                    CurrentPage = paginatedPaymentsResult.CurrentPage,
                    PageSize = paginatedPaymentsResult.PageSize,
                    NumberOfPages = paginatedPaymentsResult.NumberOfPages
                };

                var result = new GoodBoyDashboardStatsDto
                {
                    TraderCount = traderCount,
                    TotalLevies = totalLevies,
                    Payments = paginatedPaymentsForDto
                };

                await CreateAuditLog(
                    "Dashboard Stats Retrieved",
                    $"CorrelationId: {correlationId} - Dashboard stats retrieved for GoodBoy ID: {goodBoyId}, " +
                    $"Page {paginationFilter.PageNumber}, Size {paginationFilter.PageSize}",
                    "Levy Management"
                );

                return ResponseFactory.Success(result, "Dashboard statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Dashboard Stats Query Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<GoodBoyDashboardStatsDto>(ex, "An unexpected error occurred");
            }
        }

        /* public async Task<BaseResponse<GoodBoyDashboardStatsDto>> GetDashboardStats(
     string goodBoyId,
     DateTime? fromDate = null,
     DateTime? toDate = null,
     string searchQuery = null,
     PaginationFilter paginationFilter = null)
         {
             var correlationId = Guid.NewGuid().ToString();
             var userId = _currentUser.GetUserId();
             try
             {
                 await CreateAuditLog(
                     "Dashboard Stats Query",
                     $"CorrelationId: {correlationId} - Retrieving dashboard stats for GoodBoy ID: {goodBoyId}",
                     "Levy Management"
                 );

                 // Set default date range if not provided (current month)
                 if (!fromDate.HasValue || !toDate.HasValue)
                 {
                     fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                     toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                 }

                 // Set default pagination if not provided
                 paginationFilter ??= new PaginationFilter();

                 // Get trader count managed by the good boy
                 var traderCount = await _repository.TraderRepository.GetTraderCountByGoodBoyIdAsync(goodBoyId);

                 // Get total levy amount collected by the good boy in the date range
                 var totalLevies = await _repository.LevyPaymentRepository.GetTotalLevyAmountByGoodBoyIdAsync(
                     userId,
                     fromDate.Value,
                     toDate.Value
                 );

                 // Get levy payments for the good boy using the existing GetLevyPaymentsByDateRangeAsync method
                 var payments = await _repository.LevyPaymentRepository.GetLevyPaymentsByDateRangeAsync(
                     userId,
                     fromDate.Value,
                     toDate.Value
                 );

                 // Filter payments by search query if provided
                 if (!string.IsNullOrWhiteSpace(searchQuery))
                 {
                     searchQuery = searchQuery.Trim().ToLower();
                     payments = payments.Where(p =>
                         (p.Trader?.User?.FirstName != null && p.Trader.User.FirstName.ToLower().Contains(searchQuery)) ||
                         (p.Trader?.User?.LastName != null && p.Trader.User.LastName.ToLower().Contains(searchQuery)) ||
                         (p.TransactionReference != null && p.TransactionReference.ToLower().Contains(searchQuery))
                     ).ToList();
                 }

                 // Apply pagination manually since we already have the full list from the repository
                 var totalCount = payments.Count();
                 var totalPages = (int)Math.Ceiling((double)totalCount / paginationFilter.PageSize);

                 var paginatedPayments = payments
                     .OrderByDescending(p => p.PaymentDate)
                     .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                     .Take(paginationFilter.PageSize)
                     .ToList();

                 // Map payment entities to DTOs
                 var paymentDtos = paginatedPayments.Select(p => new LevyPaymentDto
                 {
                     PayerName = p.Trader?.User != null
                         ? $"{p.Trader.User.FirstName} {p.Trader.User.LastName}"
                         : "Unknown Trader",
                     PaymentTime = p.PaymentDate,
                     Amount = p.Amount
                 });

                 var result = new GoodBoyDashboardStatsDto
                 {
                     TraderCount = traderCount,
                     TotalLevies = totalLevies,
                     Payments = new PaginatorDto<IEnumerable<LevyPaymentDto>>
                     {
                         PageItems = paymentDtos,
                         CurrentPage = paginationFilter.PageNumber,
                         PageSize = paginationFilter.PageSize,
                         NumberOfPages = totalPages
                     }
                 };

                 await CreateAuditLog(
                     "Dashboard Stats Retrieved",
                     $"CorrelationId: {correlationId} - Dashboard stats retrieved for GoodBoy ID: {goodBoyId}",
                     "Levy Management"
                 );

                 return ResponseFactory.Success(result, "Dashboard statistics retrieved successfully");
             }
             catch (Exception ex)
             {
                 await CreateAuditLog(
                     "Dashboard Stats Query Failed",
                     $"CorrelationId: {correlationId} - Error: {ex.Message}",
                     "Levy Management"
                 );
                 _logger.LogError(ex, "Error retrieving dashboard stats: {ErrorMessage}", ex.Message);
                 return ResponseFactory.Fail<GoodBoyDashboardStatsDto>(ex, "An unexpected error occurred");
             }
         }*/

        public async Task<BaseResponse<GoodBoyLevyPaymentResponseDto>> CollectLevyPayment(LevyPaymentCreateDto levyPaymentDto)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Levy Payment Collection",
                    $"CorrelationId: {correlationId} - Collecting levy payment from Trader ID: {levyPaymentDto.TraderId}",
                    "Levy Management"
                );

                // Validate trader exists
                var trader = await _repository.TraderRepository.GetTraderById(levyPaymentDto.TraderId, false);
                if (trader == null)
                {
                    return ResponseFactory.Fail<GoodBoyLevyPaymentResponseDto>("Trader not found");
                }

                // Create the levy payment entity
                var levyPayment = new LevyPayment
                {
                    TraderId = levyPaymentDto.TraderId,
                    GoodBoyId = levyPaymentDto.GoodBoyId,
                    MarketId = trader.MarketId,
                    Amount = levyPaymentDto.Amount,
                    Period = levyPaymentDto.Period,
                    PaymentMethod = levyPaymentDto.PaymentMethod,
                    PaymentStatus = PaymentStatusEnum.Paid,
                    TransactionReference = Guid.NewGuid().ToString(),
                    HasIncentive = levyPaymentDto.HasIncentive,
                    IncentiveAmount = levyPaymentDto.IncentiveAmount,
                    PaymentDate = DateTime.Now,
                    CollectionDate = DateTime.Now,
                    Notes = levyPaymentDto.Notes,
                    QRCodeScanned = levyPaymentDto.QRCodeScanned
                };

                 _repository.LevyPaymentRepository.AddPayment(levyPayment);
                await _repository.SaveChangesAsync();

                var levyPaymentResponse = _mapper.Map<GoodBoyLevyPaymentResponseDto>(levyPayment);

                await CreateAuditLog(
                    "Levy Payment Collected",
                    $"CorrelationId: {correlationId} - Levy payment collected from Trader ID: {levyPaymentDto.TraderId}",
                    "Levy Management"
                );

                return ResponseFactory.Success(levyPaymentResponse, "Levy payment collected successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "Levy Payment Collection Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Levy Management"
                );
                return ResponseFactory.Fail<GoodBoyLevyPaymentResponseDto>(ex, "An unexpected error occurred");
            }
        }
    }
}

