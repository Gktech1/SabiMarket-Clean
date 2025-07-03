using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IServices;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Exceptions;
using AutoMapper;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Utilities;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Domain.Entities;
using Microsoft.AspNetCore.Http;
using ValidationException = FluentValidation.ValidationException;
using SabiMarket.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.DTOs.MarketParticipants;

namespace SabiMarket.Infrastructure.Services
{
    public class CaretakerService : ICaretakerService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<CaretakerService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IValidator<LoginRequestDto> _loginValidator;
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
        private readonly IValidator<CreateGoodBoyDto> _createGoodBoyValidator;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<CaretakerForCreationRequestDto> _createCaretakerValidator;
        private readonly ICloudinaryService _cloudinaryService;


        public CaretakerService(
            IRepositoryManager repository,
            ILogger<CaretakerService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IValidator<LoginRequestDto> loginValidator,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<UpdateProfileDto> updateProfileValidator,
            IValidator<CreateGoodBoyDto> createGoodBoyValidator,
            ICurrentUserService currentUser = null,
            IHttpContextAccessor httpContextAccessor = null,
            IValidator<CaretakerForCreationRequestDto> createCaretakerValidator = null,
            ICloudinaryService cloudinaryService = null)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _loginValidator = loginValidator;
            _updateProfileValidator = updateProfileValidator;
            _createGoodBoyValidator = createGoodBoyValidator;
            _currentUser = currentUser;
            _httpContextAccessor = httpContextAccessor;
            _createCaretakerValidator = createCaretakerValidator;
            _cloudinaryService = cloudinaryService;
        }

        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }
        private async Task CreateAuditLog(string activity, string details, string module = "Chairman Management")
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

        // Caretaker Management Methods
        public async Task<BaseResponse<CaretakerResponseDto>> GetCaretakerById(string userId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(userId, trackChanges: false);
                if (caretaker == null)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                // Replace AutoMapper with manual mapping
                var caretakerDto = new CaretakerResponseDto
                {
                    Id = caretaker.Id,
                    UserId = caretaker.UserId,
                    Email = caretaker.User?.Email,
                    FirstName = caretaker.User?.FirstName ?? "Default",
                    LastName = caretaker.User?.LastName ?? "User",
                    MarketId = caretaker.MarketId,
                    PhoneNumber = caretaker.User?.PhoneNumber,
                    ProfileImageUrl = caretaker.User?.ProfileImageUrl ?? "",
                    IsActive = caretaker.User?.IsActive ?? false,
                    CreatedAt = caretaker.CreatedAt,
                    UpdatedAt = caretaker.UpdatedAt,
                    IsBlocked = caretaker.IsBlocked
                };

                // Map Market information if available
                if (caretaker.Markets != null && caretaker.Markets.Any())
                {
                    var primaryMarket = caretaker.Markets.FirstOrDefault();
                    if (primaryMarket != null)
                    {
                        caretakerDto.Market = new MarketResponseDto
                        {
                            Id = primaryMarket.Id,
                            MarketName = primaryMarket.MarketName,
                            Location = primaryMarket.Location,
                            Description = primaryMarket.Description,
                            TotalTraders = primaryMarket.TotalTraders,
                            Capacity = primaryMarket.Capacity,
                            //ContactPhone = primaryMarket.ContactPhone,
                            // ContactEmail = primaryMarket.ContactEmail,
                            CreatedAt = primaryMarket.CreatedAt,
                            UpdatedAt = primaryMarket.UpdatedAt,
                            CaretakerId = primaryMarket.CaretakerId
                        };
                    }
                }

                return ResponseFactory.Success(caretakerDto, "Caretaker retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving caretaker");
                return ResponseFactory.Fail<CaretakerResponseDto>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<CaretakerResponseDto>> CreateCaretaker(CaretakerForCreationRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();

            try
            {
                // Validate request
                var validationResult = await _createCaretakerValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Invalid caretaker data"
                    );
                }

                // Check if user already exists by email
                var existingUser = await _userManager.FindByEmailAsync(request.EmailAddress);
                if (existingUser != null)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new BadRequestException("Email address is already registered"),
                        "Email already exists"
                    );
                }

                // Check if the market already has a caretaker
                var existingCaretaker = await _repository.CaretakerRepository.CaretakerExists(userId, request.MarketId);
                if (existingCaretaker)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new BadRequestException("Market already has an assigned caretaker"),
                        "Caretaker already exists for this market"
                    );
                }

                // Create ApplicationUser
                var defaultPassword = GenerateDefaultPassword(request.FullName);
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = request.EmailAddress,
                    Email = request.EmailAddress,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FullName.Split(' ')[0],
                    LastName = request.FullName.Split(' ').Length > 1 ? string.Join(" ", request.FullName.Split(' ').Skip(1)) : "",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Gender = request.Gender,
                    ProfileImageUrl = request.PhotoUrl,
                    LocalGovernmentId = request.LocalGovernmentId
                };

                var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createUserResult.Succeeded)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new Exception(string.Join(", ", createUserResult.Errors.Select(e => e.Description))),
                        "Failed to create user account"
                    );
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Caretaker);
                if (!roleResult.Succeeded)
                {
                    // Rollback user creation if role assignment fails
                    await _userManager.DeleteAsync(user);
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new Exception("Failed to assign caretaker role"),
                        "Role assignment failed"
                    );
                }

                // Get Chairman details
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new NotFoundException("Chairman not found"),
                        "Chairman does not exist"
                    );
                }

                // Create Caretaker entity
                var caretaker = new Caretaker
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    MarketId = request.MarketId,
                    ChairmanId = chairman.Id,  // Set the ChairmanId here
                    LocalGovernmentId = request.LocalGovernmentId,
                    IsBlocked = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    User = user
                };

                // Add caretaker to the repository
                _repository.CaretakerRepository.CreateCaretaker(caretaker);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<CaretakerResponseDto>(caretaker);
                response.DefaultPassword = defaultPassword;

                return ResponseFactory.Success(response,
                    "Caretaker created successfully. Please note down the default password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating caretaker");
                return ResponseFactory.Fail<CaretakerResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<CaretakerResponseDto>> UpdateCaretaker(string caretakerId, UpdateCaretakerRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId();

            try
            {
                await CreateAuditLog(
                    "Caretaker Update",
                    $"CorrelationId: {correlationId} - Updating caretaker with ID: {caretakerId}",
                    "Caretaker Management"
                );

                // Get Chairman details (to verify authority)
                var chairman = await _repository.ChairmanRepository.GetChairmanById(userId, false);
                if (chairman == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Chairman not found",
                        "Caretaker Management"
                    );
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new BadRequestException("Chairman not found"),
                        "Chairman not found");
                }

                // Get existing caretaker with market assignments
                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(caretakerId, false);
                if (caretaker == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Caretaker not found",
                        "Caretaker Management"
                    );
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new NotFoundException($"Caretaker with ID {caretakerId} not found"),
                        "Caretaker not found");
                }

                // Verify this chairman has authority over this caretaker
                if (caretaker.ChairmanId != chairman.Id)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Unauthorized access",
                        "Caretaker Management"
                    );
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new UnauthorizedAccessException("You are not authorized to update this caretaker"),
                        "Unauthorized access");
                }

                // Get the actual user that EF is tracking
                var actualUser = await _userManager.FindByIdAsync(caretaker.UserId);
                if (actualUser == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Associated user not found",
                        "Caretaker Management"
                    );
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new NotFoundException("Associated user account not found"),
                        "User not found");
                }

                // Apply updates to the tracked entity
                if (!string.IsNullOrEmpty(request?.FullName) && request?.FullName != "string")
                {
                    var nameParts = request.FullName.Split(' ');
                    actualUser.FirstName = nameParts.Length > 0 ? nameParts[0] : actualUser.FirstName;
                    actualUser.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : actualUser.LastName;
                }

                if (!string.IsNullOrEmpty(request?.Email) && request?.Email != "string")
                {
                    actualUser.Email = request.Email;
                    actualUser.UserName = request.Email; // Update username to match email
                    actualUser.NormalizedEmail = request.Email.ToUpper();
                    actualUser.NormalizedUserName = request.Email.ToUpper();
                }

                if (!string.IsNullOrEmpty(request?.PhoneNumber) && request?.PhoneNumber != "string")
                {
                    actualUser.PhoneNumber = request.PhoneNumber;
                }

                if (!string.IsNullOrEmpty(request?.Gender) && request?.Gender != "string")
                {
                    actualUser.Gender = request.Gender;
                }

                if (request?.ProfileImage != null && request?.ProfileImage != "string")
                {
                    actualUser.ProfileImageUrl = request.ProfileImage;
                }

                // Update the tracked entity
                var updateUserResult = await _userManager.UpdateAsync(actualUser);
                if (!updateUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Failed to update user account",
                        "Caretaker Management"
                    );
                    return ResponseFactory.Fail<CaretakerResponseDto>(
                        new Exception(string.Join(", ", updateUserResult.Errors.Select(e => e.Description))),
                        "Failed to update user account");
                }

                // Update Caretaker details
                caretaker.UpdatedAt = DateTime.UtcNow;

                // Handle market assignment
                if (!string.IsNullOrEmpty(request?.MarketId) && request.MarketId != "string")
                {
                    // Validate that market exists and belongs to chairman's LG
                    var market = await _repository.MarketRepository.GetMarketByIdAsync(request.MarketId, false);
                    if (market != null && market.LocalGovernmentId == chairman.LocalGovernmentId)
                    {
                        caretaker.MarketId = request.MarketId;
                        // Update Local Government ID based on the market
                        caretaker.LocalGovernmentId = market.LocalGovernmentId;
                    }
                    else
                    {
                        await CreateAuditLog(
                            "Update Warning",
                            $"CorrelationId: {correlationId} - Market not found or not in chairman's jurisdiction",
                            "Caretaker Management"
                        );
                        // We don't fail the request, just log the warning
                    }
                }

                if (caretaker.User != null)
                {
                    caretaker.User = null;  // Remove the ApplicationUser from caretaker to avoid tracking conflict
                }

                _repository.CaretakerRepository.UpdateCaretaker(caretaker);
                await _repository.SaveChangesAsync();

                // Get updated caretaker with market details for response
                var updatedCaretaker = await _repository.CaretakerRepository.GetCaretakerById(caretakerId, false);

                // Get fresh user data after all updates
                var updatedUser = await _userManager.FindByIdAsync(caretaker.UserId);

                // Map response
                var response = _mapper.Map<CaretakerResponseDto>(updatedCaretaker);
                //response.FullName = $"{updatedUser.FirstName} {updatedUser.LastName}".Trim();
                response.FirstName = updatedUser.FirstName;
                response.LastName = updatedUser.LastName;
                response.Email = updatedUser.Email;
                response.PhoneNumber = updatedUser.PhoneNumber;
                response.Gender = updatedUser.Gender;
                response.ProfileImageUrl = updatedUser.ProfileImageUrl;

                await CreateAuditLog(
                    "Caretaker Updated",
                    $"CorrelationId: {correlationId} - Caretaker updated successfully with ID: {caretaker.Id}",
                    "Caretaker Management"
                );

                return ResponseFactory.Success(response, "Caretaker updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating caretaker");
                await CreateAuditLog(
                    "Update Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Caretaker Management"
                );
                return ResponseFactory.Fail<CaretakerResponseDto>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<bool>> DeleteCaretaker(string caretakerId)
        {
            try
            {
                // Verify caretaker exists
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: false);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                // Create audit log for the operation
                await CreateAuditLog(
                    "Caretaker Deletion",
                    $"Deleting caretaker with ID: {caretakerId}",
                    "Caretaker Management"
                );

                // Start transaction to ensure all operations succeed or fail together
                using (var transaction = await _repository.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. First, get and remove all GoodBoys associated with this caretaker
                        var goodBoys = await _repository.GoodBoyRepository
                            .FindByCondition(g => g.CaretakerId == caretakerId, trackChanges: false)
                            .ToListAsync();

                        if (goodBoys.Any())
                        {
                            _logger.LogInformation($"Removing {goodBoys.Count} GoodBoys associated with caretaker {caretakerId}");
                            foreach (var goodBoy in goodBoys)
                            {
                                _repository.GoodBoyRepository.DeleteGoodBoy(goodBoy);
                            }

                            // Save changes after removing GoodBoys to release foreign key constraints
                            await _repository.SaveChangesAsync();
                        }

                        // 2. Find and remove any Traders associated with this caretaker
                        var traders = await _repository.TraderRepository
                            .FindByCondition(t => t.CaretakerId == caretakerId, trackChanges: false)
                            .ToListAsync();

                        if (traders.Any())
                        {
                            _logger.LogInformation($"Removing {traders.Count} Traders associated with caretaker {caretakerId}");
                            foreach (var trader in traders)
                            {
                                _repository.TraderRepository.DeleteTrader(trader);
                            }

                            await _repository.SaveChangesAsync();
                        }

                        // 3. Remove any other dependencies if they exist
                        // (Add similar code blocks for other entities as needed)

                        // 4. Finally, delete the caretaker
                        _repository.CaretakerRepository.DeleteCaretaker(caretaker);
                        await _repository.SaveChangesAsync();

                        // 5. Delete the associated user account
                        /* var user = await _userManager.FindByIdAsync(caretaker.UserId);
                         if (user != null)
                         {
                             var deleteUserResult = await _userManager.DeleteAsync(user);
                             if (!deleteUserResult.Succeeded)
                             {
                                 throw new Exception("Failed to delete user account");
                             }
                         }*/

                        // 5. Delete the associated user account
                        var user = await _userManager.FindByIdAsync(caretaker.UserId);
                        if (user != null)
                        {
                            // First, handle any audit logs referencing this user
                            var auditLogs = await _repository.AuditLogRepository
                                        .FindByCondition(al => al.UserId == user.Id, trackChanges: false) // Add trackChanges parameter
                                        .ToListAsync();


                            if (auditLogs.Any())
                            {
                                // Option 1: Update audit logs to remove user reference
                                foreach (var log in auditLogs)
                                {
                                    log.UserId = null; // Or set to a default/system user ID
                                    _repository.AuditLogRepository.Update(log);
                                }
                                await _repository.SaveChangesAsync();
                            }

                            // Now proceed with user deletion
                            var deleteUserResult = await _userManager.DeleteAsync(user);
                            if (!deleteUserResult.Succeeded)
                            {
                                return ResponseFactory.Fail<bool>(
                       new NotFoundException("Failed to delete user account"),
                       "Failed to delete user account");
                            }
                        }

                        // 6. Commit the transaction
                        await transaction.CommitAsync();

                        await CreateAuditLog(
                            "Caretaker Deleted",
                            $"Successfully deleted caretaker with ID: {caretakerId}",
                            "Caretaker Management"
                        );

                        return ResponseFactory.Success(true, "Caretaker and related entities successfully deleted");
                    }
                    catch (Exception ex)
                    {
                        // If anything fails, roll back the transaction
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, $"Error during transaction when deleting caretaker {caretakerId}");

                        await CreateAuditLog(
                            "Deletion Failed",
                            $"Failed to delete caretaker with ID: {caretakerId}. Error: {ex.Message}",
                            "Caretaker Management"
                        );

                        throw; // Re-throw to be handled by the outer catch block
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting caretaker with ID: {caretakerId}");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred while deleting the caretaker");
            }
        }


        private async Task<bool> CheckCaretakerDependencies(Caretaker caretaker)
        {
            // Check for active marketsm => m.CaretakerId == caretaker.Id, false)

            var activeMarkets = await _repository.MarketRepository
                  .GetMarketsByCaretakerId(caretaker.Id)
                  .CountAsync();

            // Check for active good boys
            var activeGoodBoys = await _repository.GoodBoyRepository
                .FindByCondition(g => g.CaretakerId == caretaker.Id, false)
                .CountAsync();

            // Check for active traders
            var activeTraders = await _repository.TraderRepository
                .FindByCondition(t => t.CaretakerId == caretaker.Id, false)
                .CountAsync();

            // If any active dependencies exist, return true
            return activeMarkets > 0 || activeGoodBoys > 0 || activeTraders > 0;
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

        public async Task<BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>> GetCaretakers(
    PaginationFilter paginationFilter)
        {
            try
            {
                var caretakersPage = await _repository.CaretakerRepository
                    .GetCaretakersWithPagination(paginationFilter, trackChanges: false);

                // Replace AutoMapper with manual mapping
                var caretakerDtos = caretakersPage.PageItems.Select(c => new CaretakerResponseDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Email = c.User?.Email,
                    FirstName = c.User?.FirstName ?? "Default",  // Provide default if null
                    LastName = c.User?.LastName ?? "User",      // Provide default if null
                    MarketId = c.MarketId,
                    PhoneNumber = c.User?.PhoneNumber,
                    ProfileImageUrl = c.User?.ProfileImageUrl ?? "",
                    IsActive = c.User?.IsActive ?? false,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsBlocked = c.IsBlocked
                }).ToList();

                var response = new PaginatorDto<IEnumerable<CaretakerResponseDto>>
                {
                    PageItems = caretakerDtos,
                    CurrentPage = caretakersPage.CurrentPage,
                    PageSize = caretakersPage.PageSize,
                    NumberOfPages = caretakersPage.NumberOfPages
                };

                return ResponseFactory.Success(response, "Caretakers retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving caretakers");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<CaretakerResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }


        //Recent on Caretaker 
        /*  public async Task<BaseResponse<PaginatorDto<IEnumerable<CaretakerResponseDto>>>> GetCaretakers(
              PaginationFilter paginationFilter)
          {
              try
              {
                  var caretakersPage = await _repository.CaretakerRepository
                      .GetCaretakersWithPagination(paginationFilter, trackChanges: false);

                  var caretakerDtos = _mapper.Map<IEnumerable<CaretakerResponseDto>>(caretakersPage.PageItems);
                  var response = new PaginatorDto<IEnumerable<CaretakerResponseDto>>
                  {
                      PageItems = caretakerDtos,
                      CurrentPage = caretakersPage.CurrentPage,
                      PageSize = caretakersPage.PageSize,
                      NumberOfPages = caretakersPage.NumberOfPages
                  };

                  return ResponseFactory.Success(response, "Caretakers retrieved successfully");
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error retrieving caretakers");
                  return ResponseFactory.Fail<PaginatorDto<IEnumerable<CaretakerResponseDto>>>(
                      ex, "An unexpected error occurred");
              }
          }
  */
        // Trader Management Methods
        public async Task<BaseResponse<bool>> AssignTraderToCaretaker(string caretakerId, string traderId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var trader = await _repository.TraderRepository.GetTraderById(traderId, trackChanges: true);
                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found"),
                        "Trader not found");
                }

                if (trader.CaretakerId != null)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Trader is already assigned to a caretaker"),
                        "Trader already assigned");
                }

                trader.CaretakerId = caretakerId;
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Trader assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning trader");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        // Trader Management
        public async Task<BaseResponse<bool>> RemoveTraderFromCaretaker(string caretakerId, string traderId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var trader = caretaker.AssignedTraders
                    .FirstOrDefault(t => t.Id == traderId);

                if (trader == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Trader not found or not assigned to this caretaker"),
                        "Trader not found");
                }

                trader.CaretakerId = null;
                caretaker.AssignedTraders.Remove(trader);
                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "Trader removed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing trader from caretaker");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetAssignedTraders(
    string caretakerId, PaginationFilter paginationFilter)
        {
            try
            {
                // Check if caretaker exists first
                var caretakerExists = await _repository.CaretakerRepository
                    .CaretakerExistsAsync(caretakerId);

                if (!caretakerExists)
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                // Query traders directly from the database rather than through the navigation property
                var tradersQuery = _repository.TraderRepository
                    .GetTradersByCaretakerId(caretakerId);

                var paginatedTraders = await tradersQuery.Paginate(paginationFilter);
                var traderDtos = _mapper.Map<IEnumerable<TraderResponseDto>>(paginatedTraders.PageItems);

                var response = new PaginatorDto<IEnumerable<TraderResponseDto>>
                {
                    PageItems = traderDtos,
                    CurrentPage = paginatedTraders.CurrentPage,
                    PageSize = paginatedTraders.PageSize,
                    NumberOfPages = paginatedTraders.NumberOfPages
                };

                return ResponseFactory.Success(response, "Traders retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned traders");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }

        /*    public async Task<BaseResponse<PaginatorDto<IEnumerable<TraderResponseDto>>>> GetAssignedTraders(
                string caretakerId, PaginationFilter paginationFilter)
            {
                try
                {
                    var caretaker = await _repository.CaretakerRepository
                        .GetCaretakerById(caretakerId, trackChanges: false);

                    if (caretaker == null)
                    {
                        return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(
                            new NotFoundException("Caretaker not found"),
                            "Caretaker not found");
                    }

                    var tradersQuery = caretaker.AssignedTraders.AsQueryable();
                    var paginatedTraders = await tradersQuery.Paginate(paginationFilter);

                    var traderDtos = _mapper.Map<IEnumerable<TraderResponseDto>>(paginatedTraders.PageItems);
                    var response = new PaginatorDto<IEnumerable<TraderResponseDto>>
                    {
                        PageItems = traderDtos,
                        CurrentPage = paginatedTraders.CurrentPage,
                        PageSize = paginatedTraders.PageSize,
                        NumberOfPages = paginatedTraders.NumberOfPages
                    };

                    return ResponseFactory.Success(response, "Traders retrieved successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving assigned traders");
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<TraderResponseDto>>>(
                        ex, "An unexpected error occurred");
                }
            }
    */
        // Levy Management
        public async Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>>> GetLevyPayments(
            string caretakerId, PaginationFilter paginationFilter)
        {
            try
            {
                var levyPayments = await _repository.CaretakerRepository
                    .GetLevyPayments(caretakerId, paginationFilter, trackChanges: false);

                if (levyPayments == null)
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>>(
                        new NotFoundException("No levy payments found"),
                        "Levy payments not found");
                }

                var levyPaymentDtos = _mapper.Map<IEnumerable<GoodBoyLevyPaymentResponseDto>>(levyPayments.PageItems);
                var response = new PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>
                {
                    PageItems = levyPaymentDtos,
                    CurrentPage = levyPayments.CurrentPage,
                    PageSize = levyPayments.PageSize,
                    NumberOfPages = levyPayments.NumberOfPages
                };

                return ResponseFactory.Success(response, "Levy payments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payments");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GoodBoyLevyPaymentResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<GoodBoyLevyPaymentResponseDto>> GetLevyPaymentDetails(string levyId)
        {
            try
            {
                var levyPayment = await _repository.CaretakerRepository
                    .GetLevyPaymentDetails(levyId, trackChanges: false);

                if (levyPayment == null)
                {
                    return ResponseFactory.Fail<GoodBoyLevyPaymentResponseDto>(
                        new NotFoundException("Levy payment not found"),
                        "Levy payment not found");
                }

                var levyPaymentDto = _mapper.Map<GoodBoyLevyPaymentResponseDto>(levyPayment);
                return ResponseFactory.Success(levyPaymentDto, "Levy payment details retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving levy payment details");
                return ResponseFactory.Fail<GoodBoyLevyPaymentResponseDto>(ex, "An unexpected error occurred");
            }
        }


        // GoodBoy Management
        public async Task<BaseResponse<GoodBoyResponseDto>> CreateGoodBoy(string caretakerId, CreateGoodBoyDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            var userId = _currentUser.GetUserId(); // Assuming you have a way to get the current user's ID

            try
            {
                await CreateAuditLog(
                    "GoodBoy Creation",
                    $"CorrelationId: {correlationId} - Creating new GoodBoy: {request.FullName}",
                    "GoodBoy Management"
                );

                // Get Caretaker details
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);
                if (caretaker == null)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Caretaker not found",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Email already registered",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new BadRequestException("Email address is already registered"),
                        "Email already exists");
                }

                // Parse name parts
                var nameParts = request.FullName.Trim().Split(' ', 2);
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                // Create ApplicationUser
                var defaultPassword = GenerateDefaultPassword(request.FullName);
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Gender = request.Gender ?? "",
                    LocalGovernmentId = caretaker.LocalGovernmentId,
                    ProfileImageUrl = request.ProfileImage
                };


                var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                if (!createUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Failed to create user account",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new Exception(string.Join(", ", createUserResult.Errors.Select(e => e.Description))),
                        "Failed to create user account");
                }

                // Assign role
                var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Goodboy);
                if (!roleResult.Succeeded)
                {
                    // Rollback user creation if role assignment fails
                    await _userManager.DeleteAsync(user);
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Failed to assign GoodBoy role",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new Exception("Failed to assign GoodBoy role"),
                        "Role assignment failed");
                }

                // Create GoodBoy entity
                var goodBoy = new GoodBoy
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    CaretakerId = caretaker.Id,
                    MarketId = request.MarketIds.FirstOrDefault() ?? caretaker?.MarketId ?? "",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    User = user
                };

                // Handle market assignments
                // If no specific market assignment is required
                if (request.MarketIds != null && request.MarketIds.Count > 0)
                {
                    // Simply validate the markets
                    foreach (var marketId in request.MarketIds)
                    {
                        var market = await _repository.MarketRepository.GetMarketByIdAsync(marketId, false);
                        if (market == null || market.LocalGovernmentId != caretaker.LocalGovernmentId)
                        {
                            await CreateAuditLog(
                                "Creation Warning",
                                $"CorrelationId: {correlationId} - Invalid market: {marketId}",
                                "GoodBoy Management"
                            );
                        }
                    }
                }

                _repository.GoodBoyRepository.AddGoodBoy(goodBoy);
                await _repository.SaveChangesAsync();

                // Map response
                var goodBoyResponseDto = _mapper.Map<GoodBoyResponseDto>(goodBoy);
                goodBoyResponseDto.DefaultPassword = defaultPassword;
                goodBoyResponseDto.Email = request.Email;
                goodBoyResponseDto.PhoneNumber = request.PhoneNumber;

                await CreateAuditLog(
                    "GoodBoy Created",
                    $"CorrelationId: {correlationId} - GoodBoy created successfully with ID: {goodBoy.Id}",
                    "GoodBoy Management"
                );

                return ResponseFactory.Success(goodBoyResponseDto,
                    "GoodBoy created successfully. Please note down the default password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GoodBoy");
                await CreateAuditLog(
                    "Creation Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "GoodBoy Management"
                );
                return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<GoodBoyResponseDto>> UpdateGoodBoy(string goodBoyId, UpdateGoodBoyRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();

            try
            {
                await CreateAuditLog(
                    "GoodBoy Update",
                    $"CorrelationId: {correlationId} - Updating GoodBoy with ID: {goodBoyId}",
                    "GoodBoy Management"
                );

                // Check if GoodBoy exists
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(goodBoyId, true);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - GoodBoy not found",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new NotFoundException($"GoodBoy with ID {goodBoyId} not found"),
                        "GoodBoy not found");
                }

                // Get the user associated with the GoodBoy
                var userToUpdate = await _userManager.FindByIdAsync(goodBoy.UserId);
                if (userToUpdate == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Associated user not found",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new NotFoundException("Associated user account not found"),
                        "User not found");
                }

                // Apply updates to the user entity
                if (!string.IsNullOrEmpty(request?.FullName) && request?.FullName != "string")
                {
                    var nameParts = request.FullName.Split(' ');
                    userToUpdate.FirstName = nameParts.Length > 0 ? nameParts[0] : userToUpdate.FirstName;
                    userToUpdate.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : userToUpdate.LastName;
                }

                if (!string.IsNullOrEmpty(request?.Email) && request?.Email != "string")
                {
                    // Check if email is already taken by another user
                    var existingUser = await _userManager.FindByEmailAsync(request.Email);
                    if (existingUser != null && existingUser.Id != userToUpdate.Id)
                    {
                        await CreateAuditLog(
                            "Update Failed",
                            $"CorrelationId: {correlationId} - Email already registered",
                            "GoodBoy Management"
                        );
                        return ResponseFactory.Fail<GoodBoyResponseDto>(
                            new BadRequestException("Email address is already registered"),
                            "Email already exists");
                    }

                    userToUpdate.Email = request.Email;
                    userToUpdate.UserName = request.Email; // Update username to match email
                    userToUpdate.NormalizedEmail = request.Email.ToUpper();
                    userToUpdate.NormalizedUserName = request.Email.ToUpper();
                }

                if (!string.IsNullOrEmpty(request?.PhoneNumber) && request?.PhoneNumber != "string")
                {
                    userToUpdate.PhoneNumber = request.PhoneNumber;
                }

                if (!string.IsNullOrEmpty(request?.Gender) && request?.Gender != "string")
                {
                    userToUpdate.Gender = request.Gender;
                }

                /*   // Handle profile image update if provided
                   var profileImage = request.GetProfileImage();
                   if (profileImage != null)
                   {
                       // If there's an existing image, delete it first
                       if (!string.IsNullOrEmpty(userToUpdate.ProfileImageUrl))
                       {
                           await _cloudinaryService.DeletePhotoAsync(userToUpdate.ProfileImageUrl);
                       }

                       var uploadResult = await _cloudinaryService.UploadImage(profileImage, "goodboys");
                       if (uploadResult.IsSuccessful && uploadResult.Data.ContainsKey("Url"))
                       {
                           userToUpdate.ProfileImageUrl = uploadResult.Data["Url"];
                       }
                   }
   */
                // Update user 
                userToUpdate.ProfileImageUrl = request.ProfileImage;
                var updateUserResult = await _userManager.UpdateAsync(userToUpdate);
                if (!updateUserResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Failed to update user account",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<GoodBoyResponseDto>(
                        new Exception(string.Join(", ", updateUserResult.Errors.Select(e => e.Description))),
                        "Failed to update user account");
                }

                // Update GoodBoy details
                goodBoy.UpdatedAt = DateTime.UtcNow;

                // Update MarketId if markets are provided
                if (request?.MarketIds != null && request.MarketIds.Count > 0)
                {
                    goodBoy.MarketId = request.MarketIds[0]; // First market
                }
                else
                {
                    goodBoy.MarketId = null;
                }

                // Update GoodBoy in repository
                _repository.GoodBoyRepository.UpdateGoodBoy(goodBoy);
                await _repository.SaveChangesAsync();

                // Retrieve updated GoodBoy with user details
                var updatedGoodBoy = await _repository.GoodBoyRepository.GetGoodBoyByUserId(goodBoy.UserId);

                // Map response
                var response = _mapper.Map<GoodBoyResponseDto>(updatedGoodBoy);
                response.FullName = $"{userToUpdate.FirstName} {userToUpdate.LastName}".Trim();
                response.Email = userToUpdate.Email;
                response.PhoneNumber = userToUpdate.PhoneNumber;

                await CreateAuditLog(
                    "GoodBoy Updated",
                    $"CorrelationId: {correlationId} - GoodBoy updated successfully with ID: {goodBoy.Id}",
                    "GoodBoy Management"
                );

                return ResponseFactory.Success(response, "GoodBoy updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GoodBoy");
                await CreateAuditLog(
                    "Update Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "GoodBoy Management"
                );
                return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
            }
        }



        /*  public async Task<BaseResponse<GoodBoyResponseDto>> AddGoodBoy(string caretakerId, CreateGoodBoyDto goodBoyDto)
          {
              var correlationId = Guid.NewGuid().ToString();
              try
              {
                  await CreateAuditLog(
                      "GoodBoy Creation",
                      $"CorrelationId: {correlationId} - Creating new GoodBoy: {goodBoyDto.FullName}",
                      "GoodBoy Management"
                  );

                  // Validate request
                  var validationResult = await _createGoodBoyValidator.ValidateAsync(goodBoyDto);
                  if (!validationResult.IsValid)
                  {
                      await CreateAuditLog(
                          "Creation Failed",
                          $"CorrelationId: {correlationId} - Validation failed",
                          "GoodBoy Management"
                      );
                      return ResponseFactory.Fail<GoodBoyResponseDto>(
                          new ValidationException(validationResult.Errors),
                          "Validation failed");
                  }

                  // Check if caretaker exists
                  var caretaker = await _repository.CaretakerRepository
                      .GetCaretakerById(caretakerId, trackChanges: true);
                  if (caretaker == null)
                  {
                      await CreateAuditLog(
                          "Creation Failed",
                          $"CorrelationId: {correlationId} - Caretaker not found",
                          "GoodBoy Management"
                      );
                      return ResponseFactory.Fail<GoodBoyResponseDto>(
                          new NotFoundException("Caretaker not found"),
                          "Caretaker not found");
                  }

                  // Check if email already exists
                  var existingUser = await _userManager.FindByEmailAsync(goodBoyDto.Email);
                  if (existingUser != null)
                  {
                      await CreateAuditLog(
                          "Creation Failed",
                          $"CorrelationId: {correlationId} - Email already registered",
                          "GoodBoy Management"
                      );
                      return ResponseFactory.Fail<GoodBoyResponseDto>(
                          new BadRequestException("Email address is already registered"),
                          "Email already exists");
                  }

                  // Parse name parts
                  var nameParts = goodBoyDto.FullName.Trim().Split(' ', 2);
                  var firstName = nameParts[0];
                  var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                  // Create ApplicationUser
                  var defaultPassword = GenerateDefaultPassword(goodBoyDto.FullName);
                  var user = new ApplicationUser
                  {
                      Id = Guid.NewGuid().ToString(),
                      UserName = goodBoyDto.Email,
                      Email = goodBoyDto.Email,
                      PhoneNumber = goodBoyDto.PhoneNumber,
                      FirstName = firstName,
                      LastName = lastName,
                      EmailConfirmed = true,
                      IsActive = true,
                      CreatedAt = DateTime.UtcNow,
                      Gender = "",
                      ProfileImageUrl = "",
                      LocalGovernmentId = caretaker.LocalGovernmentId
                  };

                  var createUserResult = await _userManager.CreateAsync(user, defaultPassword);
                  if (!createUserResult.Succeeded)
                  {
                      await CreateAuditLog(
                          "Creation Failed",
                          $"CorrelationId: {correlationId} - Failed to create user account",
                          "GoodBoy Management"
                      );
                      return ResponseFactory.Fail<GoodBoyResponseDto>(
                          new Exception(string.Join(", ", createUserResult.Errors.Select(e => e.Description))),
                          "Failed to create user account");
                  }

                  // Assign role
                  var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Goodboy);
                  if (!roleResult.Succeeded)
                  {
                      // Rollback user creation if role assignment fails
                      await _userManager.DeleteAsync(user);
                      await CreateAuditLog(
                          "Creation Failed",
                          $"CorrelationId: {correlationId} - Failed to assign GoodBoy role",
                          "GoodBoy Management"
                      );
                      return ResponseFactory.Fail<GoodBoyResponseDto>(
                          new Exception("Failed to assign GoodBoy role"),
                          "Role assignment failed");
                  }

                  // Create GoodBoy entity
                  var goodBoy = new GoodBoy
                  {
                      Id = Guid.NewGuid().ToString(),
                      UserId = user.Id,
                      CaretakerId = caretakerId,
                      MarketId = caretaker?.MarketId ?? "",  
                      CreatedAt = DateTime.UtcNow,
                      IsActive = true,
                      User = user
                  };

                  _repository.GoodBoyRepository.AddGoodBoy(goodBoy);
                  await _repository.SaveChangesAsync();

                  // Map response
                  var goodBoyResponseDto = _mapper.Map<GoodBoyResponseDto>(goodBoy);
                  goodBoyResponseDto.DefaultPassword = defaultPassword;

                  await CreateAuditLog(
                      "GoodBoy Created",
                      $"CorrelationId: {correlationId} - GoodBoy created successfully with ID: {goodBoy.Id}",
                      "GoodBoy Management"
                  );

                  return ResponseFactory.Success(goodBoyResponseDto,
                      "GoodBoy created successfully. Please note down the default password.");
              }
              catch (Exception ex)
              {
                  _logger.LogError(ex, "Error creating GoodBoy");
                  await CreateAuditLog(
                      "Creation Failed",
                      $"CorrelationId: {correlationId} - Error: {ex.Message}",
                      "GoodBoy Management"
                  );
                  return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
              }
          }
  */
        /*    public async Task<BaseResponse<GoodBoyResponseDto>> AddGoodBoy(string caretakerId, CreateGoodBoyDto goodBoyDto)
            {
                try
                {
                    var validationResult = await _createGoodBoyValidator.ValidateAsync(goodBoyDto);
                    if (!validationResult.IsValid)
                    {
                        return ResponseFactory.Fail<GoodBoyResponseDto>(
                            new FluentValidation.ValidationException(validationResult.Errors),
                            "Validation failed");
                    }

                    var caretaker = await _repository.CaretakerRepository
                        .GetCaretakerById(caretakerId, trackChanges: true);

                    if (caretaker == null)
                    {
                        return ResponseFactory.Fail<GoodBoyResponseDto>(
                            new NotFoundException("Caretaker not found"),
                            "Caretaker not found");
                    }

                    var nameParts = goodBoyDto.FullName.Trim().Split(' ', 2);
                    var firstName = nameParts[0];
                    var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;


                    // Create user account for GoodBoy
                    var user = new ApplicationUser
                    {
                        UserName = goodBoyDto.Email,
                        Email = goodBoyDto.Email,
                        PhoneNumber = goodBoyDto.PhoneNumber,
                        FirstName = firstName,
                        LastName = lastName
                    };

                    var password = goodBoyDto.PhoneNumber.TrimStart('0');

                    var result = await _userManager.CreateAsync(user, password);
                    if (!result.Succeeded)
                    {
                        return ResponseFactory.Fail<GoodBoyResponseDto>(
                            new BadRequestException(result.Errors.First().Description),
                            "Failed to create user account");
                    }

                    // Create GoodBoy entity
                    var goodBoy = new GoodBoy
                    {
                        UserId = user.Id,
                        CaretakerId = caretakerId,

                    };

                     _repository.GoodBoyRepository.AddGoodBoy(goodBoy);    
                     await _repository.SaveChangesAsync();

                    var goodBoyResponseDto = _mapper.Map<GoodBoyResponseDto>(goodBoy);
                    return ResponseFactory.Success(goodBoyResponseDto, "GoodBoy created successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating GoodBoy");
                    return ResponseFactory.Fail<GoodBoyResponseDto>(ex, "An unexpected error occurred");
                }
            }
    */

        public async Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(
      string caretakerId, PaginationFilter paginationFilter)
        {
            try
            {
                var goodBoys = await _repository.CaretakerRepository
                    .GetGoodBoys(caretakerId, paginationFilter, trackChanges: false);

                // Manual mapping instead of using AutoMapper
                var goodBoyDtos = goodBoys.PageItems.Select(g => new GoodBoyResponseDto
                {
                    Id = g.Id,
                    UserId = g.UserId,
                    FullName = g.User != null ? $"{g.User.FirstName} {g.User.LastName}" : "Default User",
                    Email = g.User?.Email ?? "",
                    PhoneNumber = g.User?.PhoneNumber ?? "",
                    MarketId = g.MarketId,
                    MarketName = g.Market?.MarketName ?? "Unknown Market",
                    ProfileImageUrl = g.User?.ProfileImageUrl,
                    LevyPayments = g.LevyPayments?.Select(lp => new GoodBoyLevyPaymentResponseDto
                    {
                        Id = lp.Id,
                        Amount = lp.Amount,
                        PaymentDate = lp.PaymentDate,
                        Status = lp.PaymentStatus.ToString(),
                        CreatedAt = lp.CreatedAt
                    }).ToList() ?? new List<GoodBoyLevyPaymentResponseDto>()
                }).ToList();

                var response = new PaginatorDto<IEnumerable<GoodBoyResponseDto>>
                {
                    PageItems = goodBoyDtos,
                    CurrentPage = goodBoys.CurrentPage,
                    PageSize = goodBoys.PageSize,
                    NumberOfPages = goodBoys.NumberOfPages
                };

                return ResponseFactory.Success(response, "GoodBoys retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GoodBoys");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }
        /*public async Task<BaseResponse<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>> GetGoodBoys(
            string caretakerId, PaginationFilter paginationFilter)
        {
            try
            {
                var goodBoys = await _repository.CaretakerRepository
                    .GetGoodBoys(caretakerId, paginationFilter, trackChanges: false);

                var goodBoyDtos = _mapper.Map<IEnumerable<GoodBoyResponseDto>>(goodBoys.PageItems);
                var response = new PaginatorDto<IEnumerable<GoodBoyResponseDto>>
                {
                    PageItems = goodBoyDtos,
                    CurrentPage = goodBoys.CurrentPage,
                    PageSize = goodBoys.PageSize,
                    NumberOfPages = goodBoys.NumberOfPages
                };

                return ResponseFactory.Success(response, "GoodBoys retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving GoodBoys");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<GoodBoyResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }*/

        public async Task<BaseResponse<bool>> BlockGoodBoy(string caretakerId, string goodBoyId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var goodBoy = caretaker.GoodBoys
                    .FirstOrDefault(gb => gb.Id == goodBoyId);

                if (goodBoy == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                // Instead of removing, we update the status
                goodBoy.Status = StatusEnum.Blocked;  // You'd need to define this enum

                // Optionally disable the user account
                var user = await _userManager.FindByIdAsync(goodBoy.UserId);
                if (user != null)
                {
                    user.LockoutEnd = DateTimeOffset.MaxValue; // Or some other blocking mechanism
                    await _userManager.UpdateAsync(user);
                }

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "GoodBoy blocked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking GoodBoy");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> UnblockGoodBoy(string caretakerId, string goodBoyId)
        {
            try
            {
                var caretaker = await _repository.CaretakerRepository
                    .GetCaretakerById(caretakerId, trackChanges: true);

                if (caretaker == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Caretaker not found"),
                        "Caretaker not found");
                }

                var goodBoy = caretaker.GoodBoys
                    .FirstOrDefault(gb => gb.Id == goodBoyId);

                if (goodBoy == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                // Update the status to Active
                goodBoy.Status = StatusEnum.Unlocked; // Ensure the enum has an Active state

                // Optionally re-enable the user account
                var user = await _userManager.FindByIdAsync(goodBoy.UserId);
                if (user != null)
                {
                    user.LockoutEnd = null; // Remove the lockout
                    await _userManager.UpdateAsync(user);
                }

                await _repository.SaveChangesAsync();

                return ResponseFactory.Success(true, "GoodBoy unblocked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking GoodBoy");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> DeleteGoodBoyByCaretaker(string goodBoyId)
        {
            var correlationId = Guid.NewGuid().ToString();
            var caretakerId = _currentUser.GetUserId();
            try
            {
                // First verify the admin exists and has proper permissions
                var caretaker = await _repository.CaretakerRepository.GetCaretakerById(caretakerId, trackChanges: false);
                if (caretaker == null)
                {
                    await CreateAuditLog(
                        "GoodBoy Deletion Failed",
                        $"CorrelationId: {correlationId} - Admin not found with ID: {caretakerId}",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("Admin not found"),
                        "Admin not found");
                }

                // Now get the goodboy to delete
                var goodBoy = await _repository.GoodBoyRepository.GetGoodBoyById(goodBoyId, trackChanges: true);
                if (goodBoy == null)
                {
                    await CreateAuditLog(
                        "GoodBoy Deletion Failed",
                        $"CorrelationId: {correlationId} - GoodBoy not found with ID: {goodBoyId}",
                        "GoodBoy Management"
                    );
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("GoodBoy not found"),
                        "GoodBoy not found");
                }

                // Check if there are any dependencies before deletion
                var hasActiveDependencies = await CheckGoodBoyDependencies(goodBoy);
                if (hasActiveDependencies)
                {
                    await CreateAuditLog(
                        "GoodBoy Deletion Failed",
                        $"CorrelationId: {correlationId} - GoodBoy has active dependencies",
                        "GoodBoy Management"
                    );
                    // Uncomment if you want to prevent deletion when dependencies exist
                    /*return ResponseFactory.Fail<bool>(
                        new InvalidOperationException("GoodBoy has active dependencies"),
                        "Cannot delete GoodBoy with active dependencies");*/
                }

                // Get associated user
                var user = await _userManager.FindByIdAsync(goodBoy.UserId);
                if (user != null)
                {
                    // Remove goodboy role from user
                    await _userManager.RemoveFromRoleAsync(user, UserRoles.Goodboy);

                    // Update user status if needed
                    // user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }

                // Delete goodboy
                _repository.GoodBoyRepository.DeleteGoodBoy(goodBoy);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "GoodBoy Deleted",
                    $"CorrelationId: {correlationId} - Admin {caretakerId} successfully deleted GoodBoy with ID: {goodBoyId}",
                    "GoodBoy Management"
                );

                return ResponseFactory.Success(true, "GoodBoy deleted successfully");
            }
            catch (Exception ex)
            {
                await CreateAuditLog(
                    "GoodBoy Deletion Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "GoodBoy Management"
                );
                _logger.LogError(ex, "Error deleting GoodBoy: {GoodBoyId} by admin: {AdminId}", goodBoyId, caretakerId);
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred while deleting the GoodBoy");
            }
        }

        // Helper method to check for any dependencies
        private async Task<bool> CheckGoodBoyDependencies(GoodBoy goodBoy)
        {
            // Check if goodboy has any active levy payments
            var hasActivePayments = goodBoy.LevyPayments.Any(lp => !lp.IsActive);

            // Add any other dependency checks as needed
            // For example, checking if the goodboy has any pending tasks or responsibilities

            return hasActivePayments;
        }

    }
}
