using System.Text.Json;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.Services.Interfaces;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Configuration;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Infrastructure.Utilities;
using ValidationException = FluentValidation.ValidationException;

namespace SabiMarket.Application.Services
{
    public class AdvertisementService : IAdvertisementService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<AdvertisementService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<CreateAdvertisementRequestDto> _createAdvertValidator;
        private readonly IValidator<UpdateAdvertisementRequestDto> _updateAdvertValidator;
        private readonly IValidator<UploadPaymentProofRequestDto> _uploadPaymentProofValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IBankSettingsService _bankSettingsService;

        public AdvertisementService(
            IRepositoryManager repository,
            ILogger<AdvertisementService> logger,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            IValidator<CreateAdvertisementRequestDto> createAdvertValidator,
            IValidator<UpdateAdvertisementRequestDto> updateAdvertValidator,
            IValidator<UploadPaymentProofRequestDto> uploadPaymentProofValidator,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ICloudinaryService cloudinaryService,
            IBankSettingsService bankSettingsService)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _currentUser = currentUser;
            _createAdvertValidator = createAdvertValidator;
            _updateAdvertValidator = updateAdvertValidator;
            _uploadPaymentProofValidator = uploadPaymentProofValidator;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _cloudinaryService = cloudinaryService;
            _bankSettingsService = bankSettingsService;
        }
        private string GetCurrentIpAddress()
        {
            return _httpContextAccessor.GetRemoteIPAddress();
        }

        private async Task CreateAuditLog(string activity, string details, string module = "Advertisement Management")
        {
            var userId = _currentUser.GetUserId();
            if (userId == null)
            {
                return;
            }
            var auditLog = new AuditLog
            {
                UserId = userId ?? "",
                Activity = activity,
                Module = module,
                Details = details,
                IpAddress = GetCurrentIpAddress()
            };
            auditLog.SetDateTime(DateTime.UtcNow);

            _repository.AuditLogRepository.Create(auditLog);
            await _repository.SaveChangesAsync();
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> CreateAdvertisement(CreateAdvertisementRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Creation",
                    $"CorrelationId: {correlationId} - Creating new advertisement: {request.Title}",
                    "Advertisement Management"
                );

                // Validate request
                var validationResult = await _createAdvertValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get current vendor's ID
                var vendorId = _currentUser.GetVendorId();
                if (string.IsNullOrEmpty(vendorId))
                {
                    await CreateAuditLog(
                        "Creation Failed",
                        $"CorrelationId: {correlationId} - User is not a vendor",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException("User is not a vendor"),
                        "User must be a vendor to create advertisements");
                }

                // Create Advertisement entity
                var advertisement = new Advertisement
                {
                    Id = Guid.NewGuid().ToString(),
                    VendorId = vendorId,
                    AdminId = "", // Will be set when admin approves
                    Title = request.Title,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    TargetUrl = request.TargetUrl,
                    Status = AdvertStatusEnum.Pending,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Price = request.Price,
                    Language = request.Language,
                    Location = request.Location,
                    AdvertPlacement = request.AdvertPlacement,
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _repository.AdvertisementRepository.CreateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Advertisement Created",
                    $"CorrelationId: {correlationId} - Advertisement created successfully with ID: {advertisement.Id}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(response, "Advertisement created successfully. Please proceed to payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating advertisement");
                await CreateAuditLog(
                    "Creation Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> UpdateAdvertisement(UpdateAdvertisementRequestDto request)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Update",
                    $"CorrelationId: {correlationId} - Updating advertisement with ID: {request.Id}",
                    "Advertisement Management"
                );

                // Validate request
                var validationResult = await _updateAdvertValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(request.Id, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {request.Id}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {request.Id} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized
                var vendorId = _currentUser.GetVendorId();
                if (advertisement.VendorId != vendorId)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - User not authorized to update this advertisement",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to update this advertisement"),
                        "Unauthorized access");
                }

                // Check if advertisement can be updated
                if (advertisement.Status != AdvertStatusEnum.Pending && advertisement.Status != AdvertStatusEnum.Rejected)
                {
                    await CreateAuditLog(
                        "Update Failed",
                        $"CorrelationId: {correlationId} - Advertisement cannot be updated in current status: {advertisement.Status}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException($"Advertisement cannot be updated in current status: {advertisement.Status}"),
                        "Advertisement cannot be updated in its current status");
                }

                // Update advertisement
                advertisement.Title = request.Title;
                advertisement.Description = request.Description;
                advertisement.ImageUrl = request.ImageUrl;
                advertisement.TargetUrl = request.TargetUrl;
                advertisement.StartDate = request.StartDate;
                advertisement.EndDate = request.EndDate;
                advertisement.Price = request.Price;
                advertisement.Language = request.Language;
                advertisement.Location = request.Location;
                advertisement.AdvertPlacement = request.AdvertPlacement;
                advertisement.UpdatedAt = DateTime.UtcNow;

                _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Advertisement Updated",
                    $"CorrelationId: {correlationId} - Advertisement updated successfully with ID: {advertisement.Id}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(response, "Advertisement updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating advertisement with ID {Id}: {Message}", request.Id, ex.Message);
                await CreateAuditLog(
                    "Update Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementDetailResponseDto>> GetAdvertisementById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return ResponseFactory.Fail<AdvertisementDetailResponseDto>(
                        new BadRequestException("Advertisement ID is required"),
                        "Invalid ID provided");
                }

                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementDetails(id);
                if (advertisement == null)
                {
                    return ResponseFactory.Fail<AdvertisementDetailResponseDto>(
                        new NotFoundException($"Advertisement with ID {id} not found"),
                        "Advertisement not found");
                }

                var advertisementDto = _mapper.Map<AdvertisementDetailResponseDto>(advertisement);

                await CreateAuditLog(
                    "Advertisement Details Query",
                    $"Retrieved advertisement details for ID: {id}",
                    "Advertisement Query"
                );

                return ResponseFactory.Success(advertisementDto, "Advertisement retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisement with ID {Id}: {ErrorMessage}", id, ex.Message);
                return ResponseFactory.Fail<AdvertisementDetailResponseDto>(
                    ex, "An unexpected error occurred while retrieving the advertisement");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetAdvertisements(
     AdvertisementFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                // Get the vendor ID for filtering
                var vendorId = _currentUser.GetVendorId();

                // Use the new repository method that handles filtering at the database level
                var paginatedResult = await _repository.AdvertisementRepository.GetFilteredAdvertisements(
                    filterDto, vendorId, paginationFilter);

                // Map to DTOs
                var advertisementDtos = paginatedResult.PageItems.Select(a =>
                {
                    var dto = _mapper.Map<AdvertisementResponseDto>(a);
                    dto.ViewCount = a.Views?.Count ?? 0;
                    return dto;
                });

                var result = new PaginatorDto<IEnumerable<AdvertisementResponseDto>>
                {
                    PageItems = advertisementDtos,
                    PageSize = paginatedResult.PageSize,
                    CurrentPage = paginatedResult.CurrentPage,
                    NumberOfPages = paginatedResult.NumberOfPages
                };

                await CreateAuditLog(
                    "Advertisement List Query",
                    $"Retrieved advertisement list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                    "Advertisement Query"
                );

                return ResponseFactory.Success(result, "Advertisements retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisements: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(
                    ex, "An unexpected error occurred while retrieving advertisements");
            }
        }
        public async Task<BaseResponse<AdvertisementResponseDto>> UpdateAdvertisementStatus(string id, string status)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Status Update",
                    $"CorrelationId: {correlationId} - Updating status of advertisement with ID: {id} to {status}",
                    "Advertisement Management"
                );

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(id, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Status Update Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {id}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {id} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized (admin)
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    await CreateAuditLog(
                        "Status Update Failed",
                        $"CorrelationId: {correlationId} - User not authorized to update advertisement status",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to update advertisement status"),
                        "Unauthorized access");
                }

                // Update status
                if (Enum.TryParse<AdvertStatusEnum>(status, true, out var statusEnum))
                {
                    advertisement.Status = statusEnum;

                    // Set admin ID if approving
                    if (statusEnum == AdvertStatusEnum.Completed)
                    {
                        advertisement.AdminId = _currentUser.GetUserId();
                    }

                    advertisement.UpdatedAt = DateTime.UtcNow;

                    _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                    await _repository.SaveChangesAsync();

                    // Map response
                    var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                    await CreateAuditLog(
                        "Advertisement Status Updated",
                        $"CorrelationId: {correlationId} - Advertisement status updated successfully with ID: {advertisement.Id}",
                        "Advertisement Management"
                    );

                    return ResponseFactory.Success(response, $"Advertisement status updated to {status} successfully");
                }
                else
                {
                    await CreateAuditLog(
                        "Status Update Failed",
                        $"CorrelationId: {correlationId} - Invalid status value: {status}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException($"Invalid status value: {status}"),
                        "Invalid status value");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating advertisement status for ID {Id}: {Message}", id, ex.Message);
                await CreateAuditLog(
                    "Status Update Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> DeleteAdvertisement(string id)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Deletion",
                    $"CorrelationId: {correlationId} - Deleting advertisement with ID: {id}",
                    "Advertisement Management"
                );

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(id, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Deletion Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {id}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {id} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized
                var vendorId = _currentUser.GetVendorId();
                var isAdmin = _currentUser.IsAdmin();

                if (advertisement.VendorId != vendorId && !isAdmin)
                {
                    await CreateAuditLog(
                        "Deletion Failed",
                        $"CorrelationId: {correlationId} - User not authorized to delete this advertisement",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to delete this advertisement"),
                        "Unauthorized access");
                }

                // Store for response
                var responseDto = _mapper.Map<AdvertisementResponseDto>(advertisement);

                // Delete advertisement
                _repository.AdvertisementRepository.DeleteAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Advertisement Deleted",
                    $"CorrelationId: {correlationId} - Advertisement deleted successfully with ID: {id}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(responseDto, "Advertisement deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting advertisement with ID {Id}: {Message}", id, ex.Message);
                await CreateAuditLog(
                    "Deletion Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> UploadPaymentProof(UploadPaymentProofRequestDto request, IFormFile proofImage)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Payment Proof Upload",
                    $"CorrelationId: {correlationId} - Uploading payment proof for advertisement with ID: {request.AdvertisementId}",
                    "Advertisement Payment"
                );

                // Validate request
                var validationResult = await _uploadPaymentProofValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    await CreateAuditLog(
                        "Upload Failed",
                        $"CorrelationId: {correlationId} - Validation failed",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(request.AdvertisementId, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Upload Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {request.AdvertisementId}",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {request.AdvertisementId} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized
                var vendorId = _currentUser.GetVendorId();
                if (advertisement.VendorId != vendorId)
                {
                    await CreateAuditLog(
                        "Upload Failed",
                        $"CorrelationId: {correlationId} - User not authorized to upload payment proof for this advertisement",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to upload payment proof for this advertisement"),
                        "Unauthorized access");
                }

                // Upload image to Cloudinary
                var uploadResult = await _cloudinaryService.UploadImage(proofImage, "payment_proofs");
                if (!uploadResult.IsSuccessful)
                {
                    await CreateAuditLog(
                        "Upload Failed",
                        $"CorrelationId: {correlationId} - Failed to upload image to Cloudinary",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException("Failed to upload payment proof image"),
                        uploadResult.Message);
                }

                // Extract the URL from the upload result
                var imageUrl = uploadResult.Data["Url"];

                // Update advertisement with payment proof URL
                advertisement.PaymentProofUrl = imageUrl;
                advertisement.BankTransferReference = request.BankTransferReference;
                advertisement.PaymentStatus = "Pending Verification";
                advertisement.UpdatedAt = DateTime.UtcNow;

                // Create payment record if it doesn't exist
                if (advertisement.Payment == null)
                {
                    // Get bank settings from configuration
                    var bankSettings = _bankSettingsService.GetBankSettings();

                    var payment = new AdvertPayment
                    {
                        Id = Guid.NewGuid().ToString(),
                        AdvertisementId = advertisement.Id,
                        Amount = advertisement.Price,
                        PaymentMethod = "Bank Transfer",
                        BankName = bankSettings.BankName,
                        AccountNumber = bankSettings.AccountNumber,
                        AccountName = bankSettings.AccountName,
                        Status = "Pending",
                        ProofOfPaymentUrl = imageUrl,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Add payment to context
                    _context.Set<AdvertPayment>().Add(payment);
                }
                else
                {
                    // Update existing payment
                    advertisement.Payment.Status = "Pending";
                    advertisement.Payment.ProofOfPaymentUrl = imageUrl;
                    advertisement.Payment.UpdatedAt = DateTime.UtcNow;
                }

                _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Payment Proof Uploaded",
                    $"CorrelationId: {correlationId} - Payment proof uploaded successfully for advertisement with ID: {advertisement.Id}",
                    "Advertisement Payment"
                );

                return ResponseFactory.Success(response, "Payment proof uploaded successfully. Awaiting verification.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading payment proof for advertisement ID {Id}: {Message}", request.AdvertisementId, ex.Message);
                await CreateAuditLog(
                    "Upload Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Payment"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<AdvertisementResponseDto>> ApprovePayment(string advertisementId)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Payment Approval",
                    $"CorrelationId: {correlationId} - Approving payment for advertisement with ID: {advertisementId}",
                    "Advertisement Payment"
                );

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(advertisementId, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Approval Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {advertisementId}",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {advertisementId} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized (admin)
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    await CreateAuditLog(
                        "Approval Failed",
                        $"CorrelationId: {correlationId} - User not authorized to approve payment",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to approve payment"),
                        "Unauthorized access");
                }

                // Check if payment proof exists
                if (string.IsNullOrEmpty(advertisement.PaymentProofUrl))
                {
                    await CreateAuditLog(
                        "Approval Failed",
                        $"CorrelationId: {correlationId} - No payment proof found for this advertisement",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException("No payment proof found for this advertisement"),
                        "No payment proof found");
                }

                // Update advertisement payment status
                advertisement.PaymentStatus = "Verified";
                advertisement.Status = AdvertStatusEnum.Active; // Activate the advertisement
                advertisement.AdminId = _currentUser.GetUserId(); // Set approving admin
                advertisement.UpdatedAt = DateTime.UtcNow;

                // Update payment record if it exists
                if (advertisement.Payment != null)
                {
                    advertisement.Payment.Status = "Verified";
                    advertisement.Payment.UpdatedAt = DateTime.UtcNow;
                    advertisement.Payment.ConfirmedAt = DateTime.UtcNow;
                }

                _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Payment Approved",
                    $"CorrelationId: {correlationId} - Payment approved successfully for advertisement with ID: {advertisement.Id}",
                    "Advertisement Payment"
                );

                return ResponseFactory.Success(response, "Payment approved successfully. Advertisement is now active.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving payment for advertisement ID {Id}: {Message}", advertisementId, ex.Message);
                await CreateAuditLog(
                    "Approval Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Payment"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> RejectPayment(string advertisementId, string reason)
        {
            var correlationId = Guid.NewGuid().ToString();
            var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(advertisementId, trackChanges: true);
            try
            {
                await CreateAuditLog(
                    "Payment Rejection",
                    $"CorrelationId: {correlationId} - Rejecting payment for advertisement with ID: {advertisementId}, Reason: {reason}",
                    "Advertisement Payment"
                );

                // Get advertisement
               // var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(advertisementId, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Rejection Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {advertisementId}",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {advertisementId} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized (admin)
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    await CreateAuditLog(
                        "Rejection Failed",
                        $"CorrelationId: {correlationId} - User not authorized to reject payment",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to reject payment"),
                        "Unauthorized access");
                }

                // Check if payment proof exists
                if (string.IsNullOrEmpty(advertisement.PaymentProofUrl))
                {
                    await CreateAuditLog(
                        "Rejection Failed",
                        $"CorrelationId: {correlationId} - No payment proof found for this advertisement",
                        "Advertisement Payment"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException("No payment proof found for this advertisement"),
                        "No payment proof found");
                }

                // Update advertisement payment status
                advertisement.PaymentStatus = "Rejected";
                advertisement.Status = AdvertStatusEnum.Pending; // Reset to pending status
                advertisement.AdminId = _currentUser.GetUserId(); // Set rejecting admin
                advertisement.UpdatedAt = DateTime.UtcNow;

                // Update payment record if it exists
                if (advertisement.Payment != null)
                {
                    advertisement.Payment.Status = "Rejected";
                    advertisement.Payment.UpdatedAt = DateTime.UtcNow;
                }

                _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Payment Rejected",
                    $"CorrelationId: {correlationId} - Payment rejected for advertisement with ID: {advertisement.Id}, Reason: {reason}",
                    "Advertisement Payment"
                );

                return ResponseFactory.Success(response, $"Payment rejected: {reason}. Please upload a valid payment proof.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting payment for advertisement ID {Id}: {Message}", advertisementId, ex.Message);
                await CreateAuditLog(
                    "Rejection Failed",
                    $"CorrelationId: {correlationId} - Rejection Failed for advertisement with ID: {advertisement.Id}, Reason: {reason}",
                    "Advertisement Payment"
                    );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetSubmittedAdvertisements(
       AdvertisementFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                // Get the vendor ID for filtering
                var vendorId = _currentUser.GetVendorId();

                // Set status to show active, pending, and completed advertisements
                filterDto.Status = "Pending,Active,Completed";

                // Use the repository's existing method for filtering
                var paginatedResult = await _repository.AdvertisementRepository.GetFilteredAdvertisements(
                    filterDto, vendorId, paginationFilter);

                // Map to DTOs
                var advertisementDtos = paginatedResult.PageItems.Select(a =>
                {
                    var dto = _mapper.Map<AdvertisementResponseDto>(a);
                    dto.ViewCount = a.Views?.Count ?? 0;
                    return dto;
                });

                var result = new PaginatorDto<IEnumerable<AdvertisementResponseDto>>
                {
                    PageItems = advertisementDtos,
                    PageSize = paginatedResult.PageSize,
                    CurrentPage = paginatedResult.CurrentPage,
                    NumberOfPages = paginatedResult.NumberOfPages
                };

                await CreateAuditLog(
                    "Submitted Advertisements Query",
                    $"Retrieved submitted advertisement list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                    "Advertisement Query"
                );

                return ResponseFactory.Success(result, "Submitted advertisements retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving submitted advertisements: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(
                    ex, "An unexpected error occurred while retrieving submitted advertisements");
            }
        }
        public async Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetArchivedAdvertisements(
     AdvertisementFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                // Get the vendor ID for filtering
                var vendorId = _currentUser.GetVendorId();

                // Set status to show only archived advertisements
                filterDto.Status = "Archived";

                // Use the repository's existing method for filtering
                var paginatedResult = await _repository.AdvertisementRepository.GetFilteredAdvertisements(
                    filterDto, vendorId, paginationFilter);

                // Map to DTOs
                var advertisementDtos = paginatedResult.PageItems.Select(a =>
                {
                    var dto = _mapper.Map<AdvertisementResponseDto>(a);
                    dto.ViewCount = a.Views?.Count ?? 0;
                    return dto;
                });

                var result = new PaginatorDto<IEnumerable<AdvertisementResponseDto>>
                {
                    PageItems = advertisementDtos,
                    PageSize = paginatedResult.PageSize,
                    CurrentPage = paginatedResult.CurrentPage,
                    NumberOfPages = paginatedResult.NumberOfPages
                };

                await CreateAuditLog(
                    "Archived Advertisements Query",
                    $"Retrieved archived advertisement list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                    "Advertisement Query"
                );

                return ResponseFactory.Success(result, "Archived advertisements retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving archived advertisements: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(
                    ex, "An unexpected error occurred while retrieving archived advertisements");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> ArchiveAdvertisement(string id)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Archiving",
                    $"CorrelationId: {correlationId} - Archiving advertisement with ID: {id}",
                    "Advertisement Management"
                );

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(id, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Archiving Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {id}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {id} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized
                var vendorId = _currentUser.GetVendorId();
                var isAdmin = _currentUser.IsAdmin();

                if (advertisement.VendorId != vendorId && !isAdmin)
                {
                    await CreateAuditLog(
                        "Archiving Failed",
                        $"CorrelationId: {correlationId} - User not authorized to archive this advertisement",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to archive this advertisement"),
                        "Unauthorized access");
                }

                // Update status to archived
                advertisement.Status = AdvertStatusEnum.Archived;
                advertisement.UpdatedAt = DateTime.UtcNow;

                _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Advertisement Archived",
                    $"CorrelationId: {correlationId} - Advertisement archived successfully with ID: {advertisement.Id}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(response, "Advertisement archived successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving advertisement with ID {Id}: {Message}", id, ex.Message);
                await CreateAuditLog(
                    "Archiving Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<AdvertisementResponseDto>> RestoreAdvertisement(string id)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Advertisement Restoration",
                    $"CorrelationId: {correlationId} - Restoring advertisement with ID: {id}",
                    "Advertisement Management"
                );

                // Get advertisement
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(id, trackChanges: true);
                if (advertisement == null)
                {
                    await CreateAuditLog(
                        "Restoration Failed",
                        $"CorrelationId: {correlationId} - Advertisement not found with ID: {id}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new NotFoundException($"Advertisement with ID {id} not found"),
                        "Advertisement not found");
                }

                // Check if user is authorized
                var vendorId = _currentUser.GetVendorId();
                var isAdmin = _currentUser.IsAdmin();

                if (advertisement.VendorId != vendorId && !isAdmin)
                {
                    await CreateAuditLog(
                        "Restoration Failed",
                        $"CorrelationId: {correlationId} - User not authorized to restore this advertisement",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new UnauthorizedException("You are not authorized to restore this advertisement"),
                        "Unauthorized access");
                }

                // Check if advertisement is archived
                if (advertisement.Status != AdvertStatusEnum.Archived)
                {
                    await CreateAuditLog(
                        "Restoration Failed",
                        $"CorrelationId: {correlationId} - Advertisement is not archived: {advertisement.Status}",
                        "Advertisement Management"
                    );
                    return ResponseFactory.Fail<AdvertisementResponseDto>(
                        new BadRequestException("Only archived advertisements can be restored"),
                        "Advertisement is not archived");
                }

                // Update status to active
                advertisement.Status = AdvertStatusEnum.Active;
                advertisement.UpdatedAt = DateTime.UtcNow;

                _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                await _repository.SaveChangesAsync();

                // Map response
                var response = _mapper.Map<AdvertisementResponseDto>(advertisement);

                await CreateAuditLog(
                    "Advertisement Restored",
                    $"CorrelationId: {correlationId} - Advertisement restored successfully with ID: {advertisement.Id}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(response, "Advertisement restored and set to active successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring advertisement with ID {Id}: {Message}", id, ex.Message);
                await CreateAuditLog(
                    "Restoration Failed",
                    $"CorrelationId: {correlationId} - Error: {ex.Message}",
                    "Advertisement Management"
                );
                return ResponseFactory.Fail<AdvertisementResponseDto>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<BulkOperationResultDto>> BulkApproveAdvertisements(List<string> advertisementIds)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Bulk Advertisement Approval",
                    $"CorrelationId: {correlationId} - Bulk approving {advertisementIds.Count} advertisements",
                    "Advertisement Management"
                );

                // Check if user is authorized (admin)
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<BulkOperationResultDto>(
                        new UnauthorizedException("You are not authorized to approve advertisements"),
                        "Unauthorized access");
                }

                var results = new BulkOperationResultDto();
                var adminId = _currentUser.GetUserId();

                foreach (var id in advertisementIds)
                {
                    try
                    {
                        var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(id, trackChanges: true);
                        if (advertisement == null)
                        {
                            results.Failed.Add(new BulkOperationItemResult
                            {
                                Id = id,
                                Error = "Advertisement not found"
                            });
                            continue;
                        }

                        // Check if payment is verified
                        if (advertisement.PaymentStatus != "Verified")
                        {
                            results.Failed.Add(new BulkOperationItemResult
                            {
                                Id = id,
                                Error = "Payment not verified"
                            });
                            continue;
                        }

                        advertisement.Status = AdvertStatusEnum.Active;
                        advertisement.AdminId = adminId;
                        advertisement.UpdatedAt = DateTime.UtcNow;

                        _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                        results.Successful.Add(new BulkOperationItemResult { Id = id });
                    }
                    catch (Exception ex)
                    {
                        results.Failed.Add(new BulkOperationItemResult
                        {
                            Id = id,
                            Error = ex.Message
                        });
                    }
                }

                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Bulk Approval Completed",
                    $"CorrelationId: {correlationId} - Approved {results.Successful.Count} out of {advertisementIds.Count} advertisements",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(results, $"Bulk operation completed. {results.Successful.Count} approved, {results.Failed.Count} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk approval of advertisements");
                return ResponseFactory.Fail<BulkOperationResultDto>(ex, "An unexpected error occurred during bulk approval");
            }
        }

        public async Task<BaseResponse<BulkOperationResultDto>> BulkRejectAdvertisements(List<string> advertisementIds, string reason)
        {
            var correlationId = Guid.NewGuid().ToString();
            try
            {
                await CreateAuditLog(
                    "Bulk Advertisement Rejection",
                    $"CorrelationId: {correlationId} - Bulk rejecting {advertisementIds.Count} advertisements. Reason: {reason}",
                    "Advertisement Management"
                );

                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<BulkOperationResultDto>(
                        new UnauthorizedException("You are not authorized to reject advertisements"),
                        "Unauthorized access");
                }

                var results = new BulkOperationResultDto();
                var adminId = _currentUser.GetUserId();

                foreach (var id in advertisementIds)
                {
                    try
                    {
                        var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(id, trackChanges: true);
                        if (advertisement == null)
                        {
                            results.Failed.Add(new BulkOperationItemResult
                            {
                                Id = id,
                                Error = "Advertisement not found"
                            });
                            continue;
                        }

                        advertisement.Status = AdvertStatusEnum.Rejected;
                        advertisement.AdminId = adminId;
                        advertisement.UpdatedAt = DateTime.UtcNow;

                        _repository.AdvertisementRepository.UpdateAdvertisement(advertisement);
                        results.Successful.Add(new BulkOperationItemResult { Id = id });
                    }
                    catch (Exception ex)
                    {
                        results.Failed.Add(new BulkOperationItemResult
                        {
                            Id = id,
                            Error = ex.Message
                        });
                    }
                }

                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Bulk Rejection Completed",
                    $"CorrelationId: {correlationId} - Rejected {results.Successful.Count} out of {advertisementIds.Count} advertisements",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(results, $"Bulk operation completed. {results.Successful.Count} rejected, {results.Failed.Count} failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk rejection of advertisements");
                return ResponseFactory.Fail<BulkOperationResultDto>(ex, "An unexpected error occurred during bulk rejection");
            }
        }

        // 2. ADVERTISEMENT ANALYTICS AND STATISTICS
        public async Task<BaseResponse<AdvertisementAnalyticsDto>> GetAdvertisementAnalytics(AnalyticsFilterDto filter)
        {
            try
            {
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<AdvertisementAnalyticsDto>(
                        new UnauthorizedException("You are not authorized to view advertisement analytics"),
                        "Unauthorized access");
                }

                var analytics = await _repository.AdvertisementRepository.GetAdvertisementAnalyticsAsync(filter);

                await CreateAuditLog(
                    "Advertisement Analytics Retrieved",
                    $"Retrieved advertisement analytics for period: {filter.StartDate:yyyy-MM-dd} to {filter.EndDate:yyyy-MM-dd}",
                    "Advertisement Analytics"
                );

                return ResponseFactory.Success(analytics, "Advertisement analytics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisement analytics");
                return ResponseFactory.Fail<AdvertisementAnalyticsDto>(ex, "An unexpected error occurred while retrieving analytics");
            }
        }

        public async Task<BaseResponse<RevenueAnalyticsDto>> GetRevenueAnalytics(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<RevenueAnalyticsDto>(
                        new UnauthorizedException("You are not authorized to view revenue analytics"),
                        "Unauthorized access");
                }

                var revenue = await _repository.AdvertisementRepository.GetRevenueAnalyticsAsync(startDate, endDate);

                await CreateAuditLog(
                    "Revenue Analytics Retrieved",
                    $"Retrieved revenue analytics for period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    "Revenue Analytics"
                );

                return ResponseFactory.Success(revenue, "Revenue analytics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue analytics");
                return ResponseFactory.Fail<RevenueAnalyticsDto>(ex, "An unexpected error occurred while retrieving revenue analytics");
            }
        }

        // 3. ADMIN DASHBOARD STATISTICS
        public async Task<BaseResponse<AdvertisementDashboardStatsDto>> GetAdvertDashboardStats()
        {
            try
            {
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<AdvertisementDashboardStatsDto>(
                        new UnauthorizedException("You are not authorized to view dashboard statistics"),
                        "Unauthorized access");
                }

                var stats = await _repository.AdvertisementRepository.GetDashboardStatsAsync();

                await CreateAuditLog(
                    "Dashboard Stats Retrieved",
                    "Retrieved advertisement dashboard statistics",
                    "Dashboard Management"
                );

                return ResponseFactory.Success(stats, "Dashboard statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return ResponseFactory.Fail<AdvertisementDashboardStatsDto>(ex, "An unexpected error occurred while retrieving dashboard statistics");
            }
        }

        // 4. EXPORT FUNCTIONALITY
     /*   public async Task<BaseResponse<byte[]>> ExportAdvertisements(AdvertisementExportRequestDto request)
        {
            try
            {
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<byte[]>(
                        new UnauthorizedException("You are not authorized to export advertisements"),
                        "Unauthorized access");
                }

                var advertisements = await _repository.AdvertisementRepository.GetAdvertisementsForExportAsync(
                    request.StartDate, request.EndDate, request.Status, request.Location, request.VendorId);

                byte[] exportData;
                string formatName;

                switch (request.Format)
                {
                    case ExportFormat.Excel:
                        exportData = await ExportToExcel(advertisements);
                        formatName = "Excel";
                        break;
                    case ExportFormat.CSV:
                        exportData = await ExportToCsv(advertisements);
                        formatName = "CSV";
                        break;
                    case ExportFormat.PDF:
                        exportData = await ExportToPdf(advertisements);
                        formatName = "PDF";
                        break;
                    default:
                        exportData = await ExportToExcel(advertisements);
                        formatName = "Excel";
                        break;
                }

                await CreateAuditLog(
                    "Advertisements Exported",
                    $"Exported {advertisements.Count} advertisements in {formatName} format",
                    "Advertisement Export"
                );

                return ResponseFactory.Success(exportData, $"Advertisements exported successfully in {formatName} format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting advertisements");
                return ResponseFactory.Fail<byte[]>(ex, "An unexpected error occurred while exporting advertisements");
            }
        }
*/
        // 5. VENDOR MANAGEMENT FOR ADVERTISEMENTS
        public async Task<BaseResponse<PaginatorDto<IEnumerable<VendorAdvertisementSummaryDto>>>> GetVendorAdvertisementSummaries(
            VendorFilterDto filter, PaginationFilter paginationFilter)
        {
            try
            {
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<VendorAdvertisementSummaryDto>>>(
                        new UnauthorizedException("You are not authorized to view vendor summaries"),
                        "Unauthorized access");
                }

                var vendorSummaries = await _repository.AdvertisementRepository.GetVendorAdvertisementSummariesAsync(filter, paginationFilter);

                await CreateAuditLog(
                    "Vendor Summaries Retrieved",
                    $"Retrieved vendor advertisement summaries - Page {paginationFilter.PageNumber}, Size {paginationFilter.PageSize}",
                    "Vendor Management"
                );

                return ResponseFactory.Success(vendorSummaries, "Vendor advertisement summaries retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vendor advertisement summaries");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<VendorAdvertisementSummaryDto>>>(
                    ex, "An unexpected error occurred while retrieving vendor summaries");
            }
        }

        // 6. PAYMENT MANAGEMENT
        public async Task<BaseResponse<PaginatorDto<IEnumerable<PaymentVerificationDto>>>> GetPendingPaymentVerifications(
            PaginationFilter paginationFilter)
        {
            try
            {
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<PaymentVerificationDto>>>(
                        new UnauthorizedException("You are not authorized to view payment verifications"),
                        "Unauthorized access");
                }

                var pendingPayments = await _repository.AdvertisementRepository.GetPendingPaymentVerificationsAsync(paginationFilter);

                await CreateAuditLog(
                    "Pending Payments Retrieved",
                    $"Retrieved pending payment verifications - Page {paginationFilter.PageNumber}, Size {paginationFilter.PageSize}",
                    "Payment Management"
                );

                return ResponseFactory.Success(pendingPayments, "Pending payment verifications retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending payment verifications");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<PaymentVerificationDto>>>(
                    ex, "An unexpected error occurred while retrieving pending payments");
            }
        }

        // 7. ADVANCED FILTERING FOR ADMIN
        public async Task<BaseResponse<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>> GetAllAdvertisementsForAdmin(
            AdminAdvertisementFilterDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(
                        new UnauthorizedException("You are not authorized to view all advertisements"),
                        "Unauthorized access");
                }

                // Admin can see all advertisements (no vendor filtering)
                var paginatedResult = await _repository.AdvertisementRepository.GetFilteredAdvertisementsForAdmin(
                    filterDto, paginationFilter);

                var advertisementDtos = paginatedResult.PageItems.Select(a =>
                {
                    var dto = _mapper.Map<AdvertisementResponseDto>(a);
                    dto.ViewCount = a.Views?.Count ?? 0;
                    dto.VendorName = $"{a.Vendor?.User?.FirstName} {a.Vendor?.User?.LastName}".Trim();
                    dto.VendorEmail = a.Vendor?.User?.Email;
                    return dto;
                });

                var result = new PaginatorDto<IEnumerable<AdvertisementResponseDto>>
                {
                    PageItems = advertisementDtos,
                    PageSize = paginatedResult.PageSize,
                    CurrentPage = paginatedResult.CurrentPage,
                    NumberOfPages = paginatedResult.NumberOfPages
                };

                await CreateAuditLog(
                    "All Advertisements Retrieved",
                    $"Retrieved all advertisements for admin - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                    "Advertisement Management"
                );

                return ResponseFactory.Success(result, "All advertisements retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all advertisements for admin: {ErrorMessage}", ex.Message);
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdvertisementResponseDto>>>(
                    ex, "An unexpected error occurred while retrieving advertisements");
            }
        }

        // 8. NOTIFICATION AND ALERTS
        public async Task<BaseResponse<List<AdvertisementAlertDto>>> GetAdvertisementAlerts()
        {
            try
            {
                var isAdmin = _currentUser.IsAdmin();
                if (!isAdmin)
                {
                    return ResponseFactory.Fail<List<AdvertisementAlertDto>>(
                        new UnauthorizedException("You are not authorized to view advertisement alerts"),
                        "Unauthorized access");
                }

                var alerts = await _repository.AdvertisementRepository.GetAdvertisementAlertsAsync();

                await CreateAuditLog(
                    "Advertisement Alerts Retrieved",
                    $"Retrieved {alerts.Count} advertisement alerts",
                    "Alert Management"
                );

                return ResponseFactory.Success(alerts, "Advertisement alerts retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisement alerts");
                return ResponseFactory.Fail<List<AdvertisementAlertDto>>(ex, "An unexpected error occurred while retrieving alerts");
            }
        }

        // 9. PERFORMANCE METRICS
        public async Task<BaseResponse<AdvertisementPerformanceDto>> GetAdvertisementPerformance(string advertisementId)
        {
            try
            {
                var advertisement = await _repository.AdvertisementRepository.GetAdvertisementById(advertisementId, false);
                if (advertisement == null)
                {
                    return ResponseFactory.Fail<AdvertisementPerformanceDto>(
                        new NotFoundException($"Advertisement with ID {advertisementId} not found"),
                        "Advertisement not found");
                }

                // Check authorization - admin or owner
                var vendorId = _currentUser.GetVendorId();
                var isAdmin = _currentUser.IsAdmin();

                if (advertisement.VendorId != vendorId && !isAdmin)
                {
                    return ResponseFactory.Fail<AdvertisementPerformanceDto>(
                        new UnauthorizedException("You are not authorized to view this advertisement's performance"),
                        "Unauthorized access");
                }

                var performance = await _repository.AdvertisementRepository.GetAdvertisementPerformanceAsync(advertisementId);

                await CreateAuditLog(
                    "Advertisement Performance Retrieved",
                    $"Retrieved performance metrics for advertisement ID: {advertisementId}",
                    "Performance Analytics"
                );

                return ResponseFactory.Success(performance, "Advertisement performance retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving advertisement performance for ID {Id}", advertisementId);
                return ResponseFactory.Fail<AdvertisementPerformanceDto>(ex, "An unexpected error occurred while retrieving performance data");
            }
        }
    }
}


