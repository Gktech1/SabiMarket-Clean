using System.Text.Json;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using CloudinaryDotNet.Actions;
using FluentValidation;
using FluentValidation.Results;
using iText.Commons.Actions.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IServices;
using SabiMarket.Application.Validators;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Helpers;
using SabiMarket.Infrastructure.Repositories;
using SabiMarket.Infrastructure.Utilities;
using ValidationException = FluentValidation.ValidationException;

public class AdminService : IAdminService
{
    private readonly IRepositoryManager _repository;
    private readonly ILogger<AdminService> _logger;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IValidator<CreateAdminRequestDto> _createAdminValidator;
    private readonly IValidator<UpdateAdminProfileDto> _updateProfileValidator;
    private readonly IValidator<CreateRoleRequestDto> _createRoleValidator;
    private readonly IValidator<UpdateRoleRequestDto> _updateRoleValidator;
    private readonly ApplicationDbContext _dbContext;

    public AdminService(
        IRepositoryManager repository,
        ILogger<AdminService> logger,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor,
        IValidator<CreateAdminRequestDto> createAdminValidator,
        IValidator<UpdateAdminProfileDto> updateProfileValidator,
        IValidator<CreateRoleRequestDto> createRoleValidator,
        IValidator<UpdateRoleRequestDto> updateRoleValidator,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext dbContext)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _createAdminValidator = createAdminValidator ?? throw new ArgumentNullException(nameof(createAdminValidator));
        _updateProfileValidator = updateProfileValidator ?? throw new ArgumentNullException(nameof(updateProfileValidator));
        _createRoleValidator = createRoleValidator ?? throw new ArgumentNullException(nameof(createRoleValidator));
        _updateRoleValidator = updateRoleValidator ?? throw new ArgumentNullException(nameof(updateRoleValidator));
        _roleManager = roleManager;
        _dbContext = dbContext;
    }

    private string GetCurrentIpAddress()
    {
        return _httpContextAccessor.GetRemoteIPAddress();
    }

    private async Task CreateAuditLog(string activity, string details, string module = "Admin Management")
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

    public async Task<BaseResponse<AdminResponseDto>> GetAdminById(string adminId)
    {
        try
        {
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: false);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Admin Lookup Failed",
                    $"Failed to find admin with ID: {adminId}",
                    "Admin Query"
                );
                return ResponseFactory.Fail<AdminResponseDto>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var adminDto = _mapper.Map<AdminResponseDto>(admin);

            await CreateAuditLog(
                "Admin Lookup",
                $"Retrieved admin details for ID: {adminId}",
                "Admin Query"
            );

            return ResponseFactory.Success(adminDto, "Admin retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin");
            return ResponseFactory.Fail<AdminResponseDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<AdminResponseDto>> CreateAdmin(CreateAdminRequestDto adminDto)
    {
        try
        {
            var validationResult = await _createAdminValidator.ValidateAsync(adminDto);
            if (!validationResult.IsValid)
            {
                await CreateAuditLog(
                    "Admin Creation Failed",
                    $"Validation failed for new admin creation with email: {adminDto.Email}",
                    "Admin Creation"
                );
                return ResponseFactory.Fail<AdminResponseDto>(
                    new FluentValidation.ValidationException(validationResult.Errors),
                    "Validation failed");
            }

            var existingUser = await _userManager.FindByEmailAsync(adminDto.Email);
            if (existingUser != null)
            {
                await CreateAuditLog(
                    "Admin Creation Failed",
                    $"Email already exists: {adminDto.Email}",
                    "Admin Creation"
                );
                return ResponseFactory.Fail<AdminResponseDto>("Email already exists");
            }

            var user = new ApplicationUser
            {
                UserName = adminDto.Email,
                Email = adminDto.Email,
                FirstName = adminDto.FirstName,
                LastName = adminDto.LastName,
                PhoneNumber = adminDto.PhoneNumber,
                ProfileImageUrl = adminDto.ProfileImageUrl,
                Gender = adminDto.Gender,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createUserResult = await _userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                await CreateAuditLog(
                    "Admin Creation Failed",
                    $"Failed to create user account for: {adminDto.Email}",
                    "Admin Creation"
                );
                return ResponseFactory.Fail<AdminResponseDto>(
                    new ValidationException((IEnumerable<FluentValidation.Results.ValidationFailure>)createUserResult
                    .Errors.Select(e => e.Description)),
                    "Failed to create user");
            }

            var admin = new Admin
            {
                UserId = user.Id,
                Position = adminDto.Position,
                Department = adminDto.Department,
                AdminLevel = adminDto.AdminLevel,
                HasDashboardAccess = adminDto.HasDashboardAccess,
                HasRoleManagementAccess = adminDto.HasRoleManagementAccess,
                HasTeamManagementAccess = adminDto.HasTeamManagementAccess,
                HasAuditLogAccess = adminDto.HasAuditLogAccess,
                StatsLastUpdatedAt = DateTime.UtcNow
            };

            _repository.AdminRepository.CreateAdmin(admin);
            await _repository.SaveChangesAsync();

            await _userManager.AddToRoleAsync(user, UserRoles.Admin);

            await CreateAuditLog(
                "Created Admin Account",
                $"Created admin account for {user.Email} ({user.FirstName} {user.LastName}) " +
                $"with role {admin.AdminLevel} in {admin.Department} department"
            );

            var createdAdmin = _mapper.Map<AdminResponseDto>(admin);
            return ResponseFactory.Success(createdAdmin, "Admin created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin");
            return ResponseFactory.Fail<AdminResponseDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> UpdateAdminProfile(string adminId, UpdateAdminProfileDto profileDto)
    {
        try
        {
            var validationResult = await _updateProfileValidator.ValidateAsync(profileDto);
            if (!validationResult.IsValid)
            {
                await CreateAuditLog(
                    "Profile Update Failed",
                    $"Validation failed for admin profile update ID: {adminId}",
                    "Profile Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException(validationResult.Errors),
                    "Validation failed");
            }

            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Profile Update Failed",
                    $"Admin not found for ID: {adminId}",
                    "Profile Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Profile Update Failed",
                    $"User not found for admin ID: {adminId}",
                    "Profile Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("User not found"),
                    "User not found");
            }

            // Track changes for audit log
            var changes = new List<string>();
            if (user.FirstName != profileDto.FirstName)
                changes.Add($"First Name: {user.FirstName} → {profileDto.FirstName}");
            if (user.LastName != profileDto.LastName)
                changes.Add($"Last Name: {user.LastName} → {profileDto.LastName}");
            if (user.PhoneNumber != profileDto.PhoneNumber)
                changes.Add($"Phone: {user.PhoneNumber} → {profileDto.PhoneNumber}");
            if (admin.Position != profileDto.Position)
                changes.Add($"Position: {admin.Position} → {profileDto.Position}");
            if (admin.Department != profileDto.Department)
                changes.Add($"Department: {admin.Department} → {profileDto.Department}");

            // Update user properties
            user.FirstName = profileDto.FirstName;
            user.LastName = profileDto.LastName;
            user.PhoneNumber = profileDto.PhoneNumber;
            user.ProfileImageUrl = profileDto.ProfileImageUrl;
            user.Gender = profileDto.Gender;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                await CreateAuditLog(
                    "Profile Update Failed",
                    $"Failed to update user properties for admin ID: {adminId}",
                    "Profile Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException(
                        (IEnumerable<FluentValidation.Results.ValidationFailure>)updateUserResult.Errors.Select(e => e.Description)),
                    "Failed to update user");
            }

            // Update admin properties
            admin.Position = profileDto.Position;
            admin.Department = profileDto.Department;

            _repository.AdminRepository.UpdateAdmin(admin);

            await CreateAuditLog(
                "Updated Admin Profile",
                $"Updated profile for {user.Email}. Changes: {string.Join(", ", changes)}",
                "Profile Management"
            );

            await _repository.SaveChangesAsync();

            return ResponseFactory.Success(true, "Admin profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin profile");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<AdminResponseDto>>>> GetAdmins(
        AdminFilterRequestDto filterDto, PaginationFilter paginationFilter)
    {
        try
        {
            var query = _repository.AdminRepository.GetFilteredAdminsQuery(filterDto);
            var paginatedAdmins = await query.Paginate(paginationFilter);

            var adminDtos = _mapper.Map<IEnumerable<AdminResponseDto>>(paginatedAdmins.PageItems);
            var result = new PaginatorDto<IEnumerable<AdminResponseDto>>
            {
                PageItems = adminDtos,
                PageSize = paginatedAdmins.PageSize,
                CurrentPage = paginatedAdmins.CurrentPage,
                NumberOfPages = paginatedAdmins.NumberOfPages,
                TotalItems = paginatedAdmins.TotalItems
            };

            await CreateAuditLog(
                "Admin List Query",
                $"Retrieved admin list - Page {paginationFilter.PageNumber}, " +
                $"Size {paginationFilter.PageSize}, Filters: {JsonSerializer.Serialize(filterDto)}",
                "Admin Query"
            );

            return ResponseFactory.Success(result, "Admins retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admins");
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<AdminResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<DashboardReportDto>> GetDashboardReportDataAsync(
          string lgaFilter = null,
          string marketFilter = null,
          int? year = null,
          TimeFrame timeFrame = TimeFrame.ThisWeek)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            await CreateAuditLog(
                "Dashboard Data Request",
                $"CorrelationId: {correlationId} - Fetching dashboard data with filters: LGA={lgaFilter}, Market={marketFilter}, Year={year}, TimeFrame={timeFrame}",
            "Dashboard Management"
            );

            var dashboardData = await _repository.ReportRepository.GetDashboardReportDataAsync(
                lgaFilter,
                marketFilter,
                year,
                timeFrame);

            await CreateAuditLog(
                "Dashboard Data Retrieved",
                $"CorrelationId: {correlationId} - Dashboard data retrieved successfully",
                "Dashboard Management"
            );

            return ResponseFactory.Success(dashboardData, "Dashboard data retrieved successfully");
        }
        catch (Exception ex)
        {
            await CreateAuditLog(
                "Dashboard Data Request Failed",
                $"CorrelationId: {correlationId} - Error: {ex.Message}",
                "Dashboard Management"
            );

            return ResponseFactory.Fail<DashboardReportDto>(ex, "An unexpected error occurred while retrieving dashboard data");
        }
    }

    public async Task<BaseResponse<byte[]>> ExportReport(ReportExportRequestDto request)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            // Log the export attempt with more detailed information
            await CreateAuditLog(
                "Report Export Requested",
                $"CorrelationId: {correlationId} - Exporting {request.ReportType} report in {request.ExportFormat} format. " +
                $"Date range: {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}. " +
                $"Market: {(string.IsNullOrEmpty(request.MarketId) ? "All Markets" : request.MarketId)}. " +
                $"LGA: {(string.IsNullOrEmpty(request.LGAId) ? "All LGAs" : request.LGAId)}",
                "Report Management"
            );

            // Validate date range
            if (request.EndDate < request.StartDate)
            {
                return ResponseFactory.Fail<byte[]>(
                    new ArgumentException("End date cannot be earlier than start date"),
                    "Invalid date range provided"
                );
            }

            // Retrieve report data with all filter parameters
            var report = await _repository.ReportRepository.ExportAdminReport(
                request.StartDate,
                request.EndDate,
                request.MarketId,
                request.LGAId,
                request.TimeZone
            );

            // Map repository data to DTO
            var reportData = _mapper.Map<ReportExportDto>(report);

            // Generate appropriate export format
            byte[] resultBytes;
            string formatName;

            switch (request.ExportFormat)
            {
                case ExportFormat.Excel:
                    resultBytes = await ExcelExportHelper.GenerateMarketReport(reportData);
                    formatName = "Excel";
                    break;

                case ExportFormat.PDF:
                    resultBytes = await PdfExportHelper.GenerateMarketReport(reportData);
                    formatName = "PDF";
                    break;

                case ExportFormat.CSV:
                    resultBytes = await CsvExportHelper.GenerateMarketReport(reportData);
                    formatName = "CSV";
                    break;

                default:
                    resultBytes = await ExcelExportHelper.GenerateMarketReport(reportData);
                    formatName = "Excel";
                    break;
            }

            // Log successful export
            await CreateAuditLog(
                "Report Exported Successfully",
                $"CorrelationId: {correlationId} - Report exported in {formatName} format. " +
                $"Size: {resultBytes.Length} bytes",
                "Report Management"
            );

            return ResponseFactory.Success(resultBytes, $"Report exported successfully in {formatName} format");
        }
        catch (Exception ex)
        {
            // Log detailed error information
            await CreateAuditLog(
                "Report Export Failed",
                $"CorrelationId: {correlationId} - Error: {ex.Message}\n" +
                $"Stack Trace: {ex.StackTrace}\n" +
                $"Report Parameters: Start={request.StartDate:yyyy-MM-dd}, End={request.EndDate:yyyy-MM-dd}, " +
                $"Market={request.MarketId}, Format={request.ExportFormat}",
                "Report Management"
            );

            return ResponseFactory.Fail<byte[]>(ex, "An unexpected error occurred while generating the report");
        }
    }

    public async Task<BaseResponse<AdminDashboardStatsDto>> GetDashboardStats(string adminId)
    {
        try
        {
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Dashboard Access Failed",
                    $"Admin not found for ID: {adminId}",
                    "Dashboard Access"
                );
                return ResponseFactory.Fail<AdminDashboardStatsDto>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            if (!admin.HasDashboardAccess)
            {
                await CreateAuditLog(
                    "Dashboard Access Denied",
                    $"Access denied for admin ID: {adminId} - No dashboard access rights",
                    "Dashboard Access"
                );
                return ResponseFactory.Fail<AdminDashboardStatsDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have access to dashboard statistics");
            }

            // Update last dashboard access
            admin.LastDashboardAccess = DateTime.UtcNow;
            _repository.AdminRepository.UpdateAdmin(admin);

            // Get dashboard statistics
            var stats = await _repository.AdminRepository.GetAdminDashboardStatsAsync(adminId);
            var statsDto = _mapper.Map<AdminDashboardStatsDto>(stats);

            await CreateAuditLog(
                "Dashboard Access",
                $"Retrieved dashboard stats for admin ID: {adminId}",
                "Dashboard Access"
            );

            await _repository.SaveChangesAsync();

            return ResponseFactory.Success(statsDto, "Dashboard stats retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return ResponseFactory.Fail<AdminDashboardStatsDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> UpdateDashboardAccess(string adminId, UpdateAdminAccessDto accessDto)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Access Update Failed",
                    $"Admin not found for ID: {adminId}",
                    "Access Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Access Update Denied",
                    $"Unauthorized access update attempt for admin ID: {adminId} by user: {userId}",
                    "Access Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to update admin access");
            }

            // Track changes for audit log
            var changes = new List<string>();
            if (admin.HasDashboardAccess != accessDto.HasDashboardAccess)
                changes.Add($"Dashboard Access: {admin.HasDashboardAccess} → {accessDto.HasDashboardAccess}");
            if (admin.HasRoleManagementAccess != accessDto.HasRoleManagementAccess)
                changes.Add($"Role Management Access: {admin.HasRoleManagementAccess} → {accessDto.HasRoleManagementAccess}");
            if (admin.HasTeamManagementAccess != accessDto.HasTeamManagementAccess)
                changes.Add($"Team Management Access: {admin.HasTeamManagementAccess} → {accessDto.HasTeamManagementAccess}");
            if (admin.HasAuditLogAccess != accessDto.HasAuditLogAccess)
                changes.Add($"Audit Log Access: {admin.HasAuditLogAccess} → {accessDto.HasAuditLogAccess}");

            // Update access permissions
            admin.HasDashboardAccess = accessDto.HasDashboardAccess;
            admin.HasRoleManagementAccess = accessDto.HasRoleManagementAccess;
            admin.HasTeamManagementAccess = accessDto.HasTeamManagementAccess;
            admin.HasAuditLogAccess = accessDto.HasAuditLogAccess;

            _repository.AdminRepository.UpdateAdmin(admin);

            var user = await _userManager.FindByIdAsync(admin.UserId);
            await CreateAuditLog(
                "Updated Access Permissions",
                $"Updated access permissions for {user?.Email ?? "Unknown"}. Changes: {string.Join(", ", changes)}",
                "Access Management"
            );

            await _repository.SaveChangesAsync();
            return ResponseFactory.Success(true, "Admin access updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin access");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> DeactivateAdmin(string adminId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Admin Deactivation Failed",
                    $"Admin not found for ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Admin Deactivation Denied",
                    $"Unauthorized deactivation attempt for admin ID: {adminId} by user: {userId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to deactivate admins");
            }

            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Admin Deactivation Failed",
                    $"User not found for admin ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("User not found"),
                    "Associated user not found");
            }

            user.IsActive = false;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await CreateAuditLog(
                    "Admin Deactivation Failed",
                    $"Failed to update user status for admin ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException((IEnumerable<FluentValidation.Results.ValidationFailure>)updateResult.
                    Errors.Select(e => e.Description)),
                    "Failed to deactivate admin");
            }

            await CreateAuditLog(
                "Deactivated Admin Account",
                $"Deactivated admin account for {user.Email} ({user.FirstName} {user.LastName})",
                "Admin Management"
            );

            await _repository.SaveChangesAsync();
            return ResponseFactory.Success(true, "Admin deactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating admin");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<bool>> ReactivateAdmin(string adminId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: true);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Admin Reactivation Failed",
                    $"Admin not found for ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Admin Reactivation Denied",
                    $"Unauthorized reactivation attempt for admin ID: {adminId} by user: {userId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to reactivate admins");
            }

            var user = await _userManager.FindByIdAsync(admin.UserId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Admin Reactivation Failed",
                    $"User not found for admin ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("User not found"),
                    "Associated user not found");
            }

            user.IsActive = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await CreateAuditLog(
                    "Admin Reactivation Failed",
                    $"Failed to update user status for admin ID: {adminId}",
                    "Admin Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException((IEnumerable<FluentValidation.Results.ValidationFailure>)
                    updateResult.Errors.Select(e => e.Description)),
                    "Failed to reactivate admin");
            }

            await CreateAuditLog(
                "Reactivated Admin Account",
                $"Reactivated admin account for {user.Email} ({user.FirstName} {user.LastName})",
                "Admin Management"
            );

            await _repository.SaveChangesAsync();
            return ResponseFactory.Success(true, "Admin reactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating admin");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<AuditLogResponseDto>>>> GetAdminAuditLogs(
    string adminId, DateTime? startDate, DateTime? endDate, PaginationFilter paginationFilter)
    {
        try
        {
            var admin = await _repository.AdminRepository.GetAdminByIdAsync(adminId, trackChanges: false);
            if (admin == null)
            {
                await CreateAuditLog(
                    "Audit Log Access Failed",
                    $"Admin not found for ID: {adminId}",
                    "Audit Log Access"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                    new NotFoundException("Admin not found"),
                    "Admin not found");
            }

            if (!admin.HasAuditLogAccess)
            {
                await CreateAuditLog(
                    "Audit Log Access Denied",
                    $"Unauthorized audit log access attempt for admin ID: {adminId}",
                    "Audit Log Access"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                    new UnauthorizedException("Access denied"),
                    "You don't have access to audit logs");
            }

            var query = _repository.AdminRepository.GetAdminAuditLogsQuery(adminId, startDate, endDate);
            var paginatedLogs = await query.Paginate(paginationFilter);

            // Map to DTOs
            var logDtos = _mapper.Map<IEnumerable<AuditLogResponseDto>>(paginatedLogs.PageItems).ToList();

            // Add roles for each audit log
            foreach (var logDto in logDtos)
            {
                var user = await _userManager.FindByIdAsync(logDto.UserId);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    logDto.UserRole = roles.FirstOrDefault() ?? "Unknown";
                }
            }

            var result = new PaginatorDto<IEnumerable<AuditLogResponseDto>>
            {
                PageItems = logDtos,
                PageSize = paginatedLogs.PageSize,
                CurrentPage = paginatedLogs.CurrentPage,
                NumberOfPages = paginatedLogs.NumberOfPages,
                TotalItems = paginatedLogs.TotalItems,
            };

            await CreateAuditLog(
                "Audit Log Access",
                $"Retrieved audit logs for admin ID: {adminId}, Date Range: {startDate?.ToString("yyyy-MM-dd") ?? "Start"} to {endDate?.ToString("yyyy-MM-dd") ?? "End"}",
                "Audit Log Access"
            );

            return ResponseFactory.Success(result, "Audit logs retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin audit logs");
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<AuditLogResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }

    // Role Management Methods matching UI/UX
    /* public async Task<BaseResponse<RoleResponseDto>> GetRoleById(string roleId)
     {
         try
         {
             var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: false);
             if (role == null)
             {
                 await CreateAuditLog(
                     "Role Lookup Failed",
                     $"Failed to find role with ID: {roleId}",
                     "Role Management"
                 );
                 return ResponseFactory.Fail<RoleResponseDto>(
                     new NotFoundException("Role not found"),
                     "Role not found");
             }

             var roleDto = _mapper.Map<RoleResponseDto>(role);

             await CreateAuditLog(
                 "Role Lookup",
                 $"Retrieved role details for ID: {roleId}",
                 "Role Management"
             );

             return ResponseFactory.Success(roleDto, "Role retrieved successfully");
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Error retrieving role");
             return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
         }
     }*/

    public async Task<BaseResponse<RoleResponseDto>> GetRoleById(string roleId)
    {
        try
        {
            var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: false);
            if (role == null)
            {
                await CreateAuditLog(
                    "Role Lookup Failed",
                    $"Failed to find role with ID: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new NotFoundException("Role not found"),
                    "Role not found");
            }

            // Use the same private mapping method instead of AutoMapper
            var roleDto = MapToRoleResponseDto(role);

            await CreateAuditLog(
                "Role Lookup",
                $"Retrieved role details for ID: {roleId}",
                "Role Management"
            );
            return ResponseFactory.Success(roleDto, "Role retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role with ID: {RoleId}", roleId);
            return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
    RoleFilterRequestDto filterDto,
    PaginationFilter paginationFilter)
    {
        try
        {
            if (paginationFilter == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                    new ArgumentNullException(nameof(paginationFilter)),
                    "Pagination parameters are required");
            }

            // Create empty filter if null to get all records
            filterDto ??= new RoleFilterRequestDto();

            const int MinPageSize = 1;
            const int DefaultPageSize = 10;
            const int MaxPageSize = 100;
            paginationFilter.PageSize = paginationFilter.PageSize switch
            {
                < MinPageSize => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => paginationFilter.PageSize
            };

            var query = _repository.AdminRepository.GetFilteredRolesQuery(filterDto);
            if (query == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                    new InvalidOperationException("Query could not be created"),
                    "Failed to create roles query");
            }

            var paginatedRoles = await query.Paginate(paginationFilter);
            var roleDtos = paginatedRoles.PageItems.Select(MapToRoleResponseDto).ToList();

            var result = new PaginatorDto<IEnumerable<RoleResponseDto>>
            {
                PageItems = roleDtos,
                PageSize = paginatedRoles.PageSize,
                CurrentPage = paginatedRoles.CurrentPage,
                NumberOfPages = paginatedRoles.NumberOfPages,
                TotalItems = paginatedRoles.TotalItems,
            };

            // Create audit log with appropriate message based on whether search was used
            var searchDescription = string.IsNullOrWhiteSpace(filterDto.SearchTerm)
                ? "all roles"
                : $"roles with search: {filterDto.SearchTerm}";

            await CreateAuditLog(
                "Role List Query",
                $"Retrieved {searchDescription} - Page {paginationFilter.PageNumber}, Size {paginationFilter.PageSize}",
                "Role Management"
            );

            return ResponseFactory.Success(result, "Roles retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving roles. Filter: {@FilterDto}, Pagination: {@PaginationFilter}",
                filterDto,
                paginationFilter);
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }
    /* public async Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
         RoleFilterRequestDto filterDto,
         PaginationFilter paginationFilter)
     {
         try
         {
             if (paginationFilter == null)
             {
                 return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                     new ArgumentNullException(nameof(paginationFilter)),
                     "Pagination parameters are required");
             }

             const int MinPageSize = 1;
             const int DefaultPageSize = 10;
             const int MaxPageSize = 100;

             paginationFilter.PageSize = paginationFilter.PageSize switch
             {
                 < MinPageSize => DefaultPageSize,
                 > MaxPageSize => MaxPageSize,
                 _ => paginationFilter.PageSize
             };

             var query = _repository.AdminRepository.GetFilteredRolesQuery(filterDto);
             if (query == null)
             {
                 return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                     new InvalidOperationException("Query could not be created"),
                     "Failed to create roles query");
             }

             var paginatedRoles = await query.Paginate(paginationFilter);

             // Use private mapping method
             var roleDtos = paginatedRoles.PageItems.Select(MapToRoleResponseDto).ToList();

             var result = new PaginatorDto<IEnumerable<RoleResponseDto>>
             {
                 PageItems = roleDtos,
                 PageSize = paginatedRoles.PageSize,
                 CurrentPage = paginatedRoles.CurrentPage,
                 NumberOfPages = paginatedRoles.NumberOfPages
             };

             var response = ResponseFactory.Success(result, "Roles retrieved successfully");

             await CreateAuditLog(
                 "Role List Query",
                 $"Retrieved role list - Page {paginationFilter.PageNumber}, " +
                 $"Size {paginationFilter.PageSize}, Search: {filterDto.SearchTerm ?? "none"}",
                 "Role Management"
             );

             return response;
         }
         catch (Exception ex)
         {
             _logger.LogError(ex,
                 "Error retrieving roles. Filter: {@FilterDto}, Pagination: {@PaginationFilter}",
                 filterDto,
                 paginationFilter);
             return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                 ex, "An unexpected error occurred");
         }
     }
 */
    private static RoleResponseDto MapToRoleResponseDto(ApplicationRole role)
    {
        if (role == null) return null;

        return new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            AllPermissions = role.Permissions?
                .Select(p => p.Name)
                .ToList() ?? new List<string>(),
            IsActive = role.IsActive,
            CreatedAt = role.CreatedAt,
            CreatedBy = role.CreatedBy,
            LastModifiedAt = role.LastModifiedAt,
            LastModifiedBy = role.LastModifiedBy
        };
    }
    //Working 
    /*  public async Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
      RoleFilterRequestDto filterDto,
      PaginationFilter paginationFilter)
      {
          try
          {
              if (paginationFilter == null)
              {
                  return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                      new ArgumentNullException(nameof(paginationFilter)),
                      "Pagination parameters are required");
              }

              const int MinPageSize = 1;
              const int DefaultPageSize = 10;
              const int MaxPageSize = 100;

              paginationFilter.PageSize = paginationFilter.PageSize switch
              {
                  < MinPageSize => DefaultPageSize,
                  > MaxPageSize => MaxPageSize,
                  _ => paginationFilter.PageSize
              };

              var query = _repository.AdminRepository.GetFilteredRolesQuery(filterDto);
              if (query == null)
              {
                  return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                      new InvalidOperationException("Query could not be created"),
                      "Failed to create roles query");
              }

              *//*var paginatedRoles = await query
                          .ProjectTo<RoleResponseDto>(_mapper.ConfigurationProvider)
                          .Paginate(paginationFilter);*//*
              var paginatedRoles = await query.Paginate(paginationFilter);
              var roleDtos = _mapper.Map<IEnumerable<RoleResponseDto>>(paginatedRoles.PageItems);

              var result = new PaginatorDto<IEnumerable<RoleResponseDto>>
              {
                  PageItems = roleDtos,
                  PageSize = paginatedRoles.PageSize,
                  CurrentPage = paginatedRoles.CurrentPage,
                  NumberOfPages = paginatedRoles.NumberOfPages
              };

              var response = ResponseFactory.Success(result, "Roles retrieved successfully");

              await CreateAuditLog(
                  "Role List Query",
                  $"Retrieved role list - Page {paginationFilter.PageNumber}, " +
                  $"Size {paginationFilter.PageSize}, Search: {filterDto.SearchTerm ?? "none"}",
                  "Role Management"
              );

              return response;
          }
          catch (Exception ex)
          {
              _logger.LogError(ex,
                  "Error retrieving roles. Filter: {@FilterDto}, Pagination: {@PaginationFilter}",
                  filterDto,
                  paginationFilter);
              return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                  ex, "An unexpected error occurred");
          }
      }*/
    /*    public async Task<BaseResponse<PaginatorDto<IEnumerable<RoleResponseDto>>>> GetRoles(
            RoleFilterRequestDto filterDto, PaginationFilter paginationFilter)
        {
            try
            {
                // Default to 10 rows per page as shown in UI
                paginationFilter.PageSize = paginationFilter.PageSize <= 0 ? 10 : paginationFilter.PageSize;

                var query = _repository.AdminRepository.GetFilteredRolesQuery(filterDto);
                var paginatedRoles = await query.Paginate(paginationFilter);

                var roleDtos = _mapper.Map<IEnumerable<RoleResponseDto>>(paginatedRoles.PageItems);

                // Format response to match UI display
                var result = new PaginatorDto<IEnumerable<RoleResponseDto>>
                {
                    PageItems = roleDtos,
                    PageSize = paginatedRoles.PageSize,
                    CurrentPage = paginatedRoles.CurrentPage,
                    NumberOfPages = paginatedRoles.NumberOfPages
                };

                await CreateAuditLog(
                    "Role List Query",
                    $"Retrieved role list - Page {paginationFilter.PageNumber}, " +
                    $"Size {paginationFilter.PageSize}, Search: {filterDto.SearchTerm ?? "none"}",
                    "Role Management"
                );

                return ResponseFactory.Success(result, "Roles retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<RoleResponseDto>>>(
                    ex, "An unexpected error occurred");
            }
        }*/

    public async Task<BaseResponse<RoleResponseDto>> CreateRole(CreateRoleRequestDto createRoleDto)
    {
        try
        {
            var validationResult = await _createRoleValidator.ValidateAsync(createRoleDto);
            if (!validationResult.IsValid)
            {
                await CreateAuditLog(
                    "Role Creation Failed",
                    $"Validation failed for new role creation: {createRoleDto.Name}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new ValidationException(validationResult.Errors),
                    "Validation failed");
            }

            // Check if role exists
            if (await _repository.AdminRepository.RoleExistsAsync(createRoleDto.Name))
            {
                await CreateAuditLog(
                    "Role Creation Failed",
                    $"Role name already exists: {createRoleDto.Name}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>("Role name already exists");
            }

            // Create role with UI-specified permissions
            var role = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = createRoleDto.Name,
                NormalizedName = createRoleDto.Name.ToUpper(),
                CreatedBy = _currentUser.GetUserId(),
                LastModifiedBy = _currentUser.GetUserId(),
                Description = createRoleDto.Description,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                IsActive = true,
                Permissions = createRoleDto.Permissions.Select(p => new RolePermission
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = p,
                    IsGranted = true
                }).ToList()
            };

            await _repository.AdminRepository.CreateRoleAsync(role);
            await _repository.SaveChangesAsync();

            await CreateAuditLog(
                "Created Role",
                $"Created role {role.Name} with permissions: {string.Join(", ", createRoleDto.Permissions)}",
                "Role Management"
            );

            var responseDto = _mapper.Map<RoleResponseDto>(role);
            return ResponseFactory.Success(responseDto, "Role created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
        }
    }

    /* public async Task<BaseResponse<PaginatorDto<IEnumerable<UserResponseDto>>>> GetAllUsers(
     UserFilterRequestDto filterDto,
     PaginationFilter paginationFilter)
     {
         try
         {
             // Verify current user has permission
             var userId = _currentUser.GetUserId();
             var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
             if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
             {
                 await CreateAuditLog(
                     "User List Access Denied",
                     $"Unauthorized attempt to retrieve users by user: {userId}",
                     "User Management"
                 );
                 return ResponseFactory.Fail<PaginatorDto<IEnumerable<UserResponseDto>>>(
                     new UnauthorizedException("Access denied"),
                     "You don't have permission to view users");
             }

             if (paginationFilter == null)
             {
                 return ResponseFactory.Fail<PaginatorDto<IEnumerable<UserResponseDto>>>(
                     new ArgumentNullException(nameof(paginationFilter)),
                     "Pagination parameters are required");
             }

             // Create empty filter if null to get all records
             filterDto ??= new UserFilterRequestDto();

             // Set pagination limits
             const int MinPageSize = 1;
             const int DefaultPageSize = 10;
             const int MaxPageSize = 100;
             paginationFilter.PageSize = paginationFilter.PageSize switch
             {
                 < MinPageSize => DefaultPageSize,
                 > MaxPageSize => MaxPageSize,
                 _ => paginationFilter.PageSize
             };

             // Get users query based on filter
             var query = _repository.UserRepository.GetFilteredUsersQuery(filterDto);
             if (query == null)
             {
                 return ResponseFactory.Fail<PaginatorDto<IEnumerable<UserResponseDto>>>(
                     new InvalidOperationException("Query could not be created"),
                     "Failed to create users query");
             }

             // Paginate results
             var paginatedUsers = await query.Paginate(paginationFilter);

             // Map to response DTOs with user type information
             var userDtos = new List<UserResponseDto>();

             foreach (var user in paginatedUsers.PageItems)
             {
                 var userDto = await MapToUserResponseDto(user);
                 userDtos.Add(userDto);
             }

             var result = new PaginatorDto<IEnumerable<UserResponseDto>>
             {
                 PageItems = userDtos,
                 PageSize = paginatedUsers.PageSize,
                 CurrentPage = paginatedUsers.CurrentPage,
                 NumberOfPages = paginatedUsers.NumberOfPages
             };

             // Create audit log
             var searchDescription = string.IsNullOrWhiteSpace(filterDto.SearchTerm)
                 ? "all users"
                 : $"users with search: {filterDto.SearchTerm}";

             await CreateAuditLog(
                 "User List Query",
                 $"Retrieved {searchDescription} - Page {paginationFilter.PageNumber}, Size {paginationFilter.PageSize}",
                 "User Management"
             );

             return ResponseFactory.Success(result, "Users retrieved successfully");
         }
         catch (Exception ex)
         {
             _logger.LogError(ex,
                 "Error retrieving users. Filter: {@FilterDto}, Pagination: {@PaginationFilter}",
                 filterDto,
                 paginationFilter);

             return ResponseFactory.Fail<PaginatorDto<IEnumerable<UserResponseDto>>>(
                 ex, "An unexpected error occurred");
         }
     }*/

    public async Task<BaseResponse<PaginatorDto<IEnumerable<UserResponseDto>>>> GetAllUsers(
    UserFilterRequestDto filterDto,
    PaginationFilter paginationFilter)
    {
        try
        {
            // Verify current user has permission
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "User List Access Denied",
                    $"Unauthorized attempt to retrieve users by user: {userId}",
                    "User Management"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<UserResponseDto>>>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to view users");
            }

            if (paginationFilter == null)
            {
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<UserResponseDto>>>(
                    new ArgumentNullException(nameof(paginationFilter)),
                    "Pagination parameters are required");
            }

            // Create empty filter if null to get all records
            filterDto ??= new UserFilterRequestDto();

            // Set pagination limits
            const int MinPageSize = 1;
            const int DefaultPageSize = 10;
            const int MaxPageSize = 100;
            paginationFilter.PageSize = paginationFilter.PageSize switch
            {
                < MinPageSize => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => paginationFilter.PageSize
            };

            // Get users from UserManager and apply filters in memory
            var users = _userManager.Users.AsQueryable();

            // Apply search term to name or email if provided
            if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
            {
                var searchTerm = filterDto.SearchTerm.ToLower();
                users = users.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.UserName.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm) ||
                    (u.FirstName + " " + u.LastName).ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
                );
            }

            // Filter by active status
            if (filterDto.IsActive.HasValue)
            {
                users = users.Where(u => u.IsActive == filterDto.IsActive.Value);
            }

            // Filter by creation date range
            if (filterDto.CreatedFrom.HasValue)
            {
                users = users.Where(u => u.CreatedAt >= filterDto.CreatedFrom.Value);
            }

            if (filterDto.CreatedTo.HasValue)
            {
                var endDate = filterDto.CreatedTo.Value.AddDays(1); // Include the end date
                users = users.Where(u => u.CreatedAt < endDate);
            }

            // Include related entities using explicit loading in Entity Framework
            users = users.Include(u => u.LocalGovernment);

            // Filter by user type using navigation properties
            if (!string.IsNullOrWhiteSpace(filterDto.UserType))
            {
                switch (filterDto.UserType.ToLower())
                {
                    case "admin":
                        users = users.Include(u => u.Admin).Where(u => u.Admin != null);
                        break;
                    case "chairman":
                        users = users.Include(u => u.Chairman).Where(u => u.Chairman != null);
                        break;
                    case "trader":
                        users = users.Include(u => u.Trader).ThenInclude(t => t.Market)
                                     .Where(u => u.Trader != null);
                        break;
                    case "vendor":
                        users = users.Include(u => u.Vendor).Where(u => u.Vendor != null);
                        break;
                    case "customer":
                        users = users.Include(u => u.Customer).Where(u => u.Customer != null);
                        break;
                    case "caretaker":
                        users = users.Include(u => u.Caretaker).Where(u => u.Caretaker != null);
                        break;
                    case "goodboy":
                        users = users.Include(u => u.GoodBoy).Where(u => u.GoodBoy != null);
                        break;
                    case "assistcenterofficer":
                        users = users.Include(u => u.AssistCenterOfficer).Where(u => u.AssistCenterOfficer != null);
                        break;
                    default:
                        // Include all entities when no specific type is selected
                        users = users.Include(u => u.Admin)
                                     .Include(u => u.Chairman)
                                     .Include(u => u.Trader).ThenInclude(t => t.Market)
                                     .Include(u => u.Vendor)
                                     .Include(u => u.Customer)
                                     .Include(u => u.Caretaker)
                                     .Include(u => u.GoodBoy)
                                     .Include(u => u.AssistCenterOfficer);
                        break;
                }
            }
            else
            {
                // Include all entities when no specific type is selected
                users = users.Include(u => u.Admin)
                             .Include(u => u.Chairman)
                             .Include(u => u.Trader).ThenInclude(t => t.Market)
                             .Include(u => u.Vendor)
                             .Include(u => u.Customer)
                             .Include(u => u.Caretaker)
                             .Include(u => u.GoodBoy)
                             .Include(u => u.AssistCenterOfficer);
            }

            // Execute the query to get the count
            var totalCount = await users.CountAsync();

            // Apply pagination manually
            var pagedUsers = await users
                .OrderByDescending(u => u.CreatedAt)
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToListAsync();

            var userDtos = new List<UserResponseDto>();
            foreach (var user in pagedUsers)
            {
                // Get all role names for the user
                var roleNames = await _userManager.GetRolesAsync(user);

                // Create a list to store roles with IDs
                var rolesWithIds = new List<UserRoleDto>();

                // Get all roles with their IDs
                foreach (var roleName in roleNames)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        rolesWithIds.Add(new UserRoleDto
                        {
                            Id = role.Id,
                            Name = role.Name
                        });
                    }
                }

                var userDto = new UserResponseDto
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = roleNames.ToList(),          // Keep this for backward compatibility
                    RolesWithIds = rolesWithIds,         // Add the new collection with role IDs
                    UserType = DetermineUserType(user, roleNames)  // Pass roleNames as the second parameter
                };

                // Add entity-specific information
                if (user.Admin != null)
                {
                    userDto.Department = user.Admin.Department;
                    userDto.Position = user.Admin.Position;
                }
                else if (user.Chairman != null && user.LocalGovernment != null)
                {
                    userDto.LocalGovernmentName = user.LocalGovernment?.Name;
                }
                else if (user.Trader != null)
                {
                    userDto.MarketName = user.Trader.Market?.MarketName;
                }

                userDtos.Add(userDto);
            }

            // If filtering by role, apply that filter in memory as it can't be done in the query
            /*if (!string.IsNullOrWhiteSpace(filterDto?.RoleName))
        {
            var filteredUsers = new List<ApplicationUser>();
            foreach (var user in pagedUsers)
            {
                if (await _userManager.IsInRoleAsync(user, filterDto.RoleName))
                {
                    filteredUsers.Add(user);
                }
            }
            pagedUsers = filteredUsers;
        }*/

            // Map users to response DTOs
            // var userDtos = new List<UserResponseDto>();
            /*  foreach (var user in pagedUsers)
              {
                  var roles = await _userManager.GetRolesAsync(user);

                  var userDto = new UserResponseDto
                  {
                      Id = user.Id,
                      FullName = $"{user.FirstName} {user.LastName}",
                      Email = user.Email,
                      PhoneNumber = user.PhoneNumber,
                      IsActive = user.IsActive,
                      CreatedAt = user.CreatedAt,
                      LastLoginAt = user.LastLoginAt,
                      Roles = roles.ToList(),
                      UserType = DetermineUserType(user)
                  };

                  // Add entity-specific information
                  if (user.Admin != null)
                  {
                      userDto.Department = user.Admin.Department;
                      userDto.Position = user.Admin.Position;
                  }
                  else if (user.Chairman != null && user.LocalGovernment != null)
                  {
                      userDto.LocalGovernmentName = user.LocalGovernment?.Name;
                  }
                  else if (user.Trader != null)
                  {
                      userDto.MarketName = user.Trader.Market?.MarketName;
                  }

                  userDtos.Add(userDto);
              }*/

            // Create paginator result
            var numberOfPages = (int)Math.Ceiling(totalCount / (double)paginationFilter.PageSize);

            var result = new PaginatorDto<IEnumerable<UserResponseDto>>
            {
                PageItems = userDtos,
                PageSize = paginationFilter.PageSize,
                CurrentPage = paginationFilter.PageNumber,
                NumberOfPages = numberOfPages,
                TotalItems = totalCount,
            };

            // Create audit log
            var searchDescription = string.IsNullOrWhiteSpace(filterDto.SearchTerm)
                ? "all users"
                : $"users with search: {filterDto.SearchTerm}";

            await CreateAuditLog(
                "User List Query",
                $"Retrieved {searchDescription} - Page {paginationFilter.PageNumber}, Size {paginationFilter.PageSize}",
                "User Management"
            );

            return ResponseFactory.Success(result, "Users retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving users. Filter: {@FilterDto}, Pagination: {@PaginationFilter}",
                filterDto,
                paginationFilter);

            return ResponseFactory.Fail<PaginatorDto<IEnumerable<UserResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<AssignRoleResponseDto>> AssignUserRoleAndPermissions(string userId, string roleId, List<PermissionDto> permissions, bool removeExistingRoles = false)
    {
        var correlationId = Guid.NewGuid().ToString();
        try
        {
            // Verify current user has permission
            var currentUserId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(currentUserId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Role Assignment Denied",
                    $"CorrelationId: {correlationId} - Unauthorized role assignment attempt by user: {currentUserId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<AssignRoleResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to assign roles");
            }

            // Get the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Role Assignment Failed",
                    $"CorrelationId: {correlationId} - User not found: {userId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<AssignRoleResponseDto>(
                    new NotFoundException($"User with ID {userId} not found"),
                    "User not found");
            }

            // Get the role
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                await CreateAuditLog(
                    "Role Assignment Failed",
                    $"CorrelationId: {correlationId} - Role not found: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<AssignRoleResponseDto>(
                    new NotFoundException($"Role with ID {roleId} not found"),
                    "Role not found");
            }

            // Get current user roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Begin transaction to ensure all operations succeed or fail together
            using var transaction = await _repository.BeginTransactionAsync();
            try
            {
                // Remove user from all current roles if requested
                if (removeExistingRoles && currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        await CreateAuditLog(
                            "Role Assignment Failed",
                            $"CorrelationId: {correlationId} - Failed to remove existing roles",
                            "Role Management"
                        );
                        return ResponseFactory.Fail<AssignRoleResponseDto>(
                            new Exception($"Failed to remove existing roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}"),
                            "Failed to remove existing roles");
                    }
                }

                // Add user to new role
                var addRoleResult = await _userManager.AddToRoleAsync(user, role.Name);
                if (!addRoleResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Role Assignment Failed",
                        $"CorrelationId: {correlationId} - Failed to assign role",
                        "Role Management"
                    );
                    return ResponseFactory.Fail<AssignRoleResponseDto>(
                        new Exception($"Failed to assign role: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}"),
                        "Failed to assign role");
                }

                // Store user permissions as a JSON string in the Admin entity
                // This avoids creating a new entity while still tracking user-specific permissions
                if (permissions != null && permissions.Any())
                {
                    // Get or create admin entity for the user
                    var admin = await _repository.AdminRepository.GetAdminByUserIdAsync(user.Id, true);

                    // If admin doesn't exist, create it regardless of the role
                    if (admin == null)
                    {
                        admin = new Admin
                        {
                            UserId = user.Id,
                            AdminLevel = role.Name,
                            Department = "General",
                            Position = role.Name,
                            HasDashboardAccess = role.Name == UserRoles.Admin,
                            HasRoleManagementAccess = role.Name == UserRoles.Admin,
                            HasTeamManagementAccess = role.Name == UserRoles.Admin,
                            HasAuditLogAccess = role.Name == UserRoles.Admin,
                            HasAdvertManagementAccess = role.Name == UserRoles.Admin,
                            RegisteredLGAs = 0,
                            ActiveChairmen = 0,
                            TotalRevenue = 0,
                            StatsLastUpdatedAt = DateTime.UtcNow
                        };

                        _repository.AdminRepository.CreateAdmin(admin);
                    }

                    // Serialize the permissions to JSON and store in a custom field 
                    // (you may need to add this field to your Admin entity)
                    var permissionsJson = System.Text.Json.JsonSerializer.Serialize(permissions);

                    // Store JSON in Description field of another entity, or in custom property
                    // This is a workaround since we can't add new entity

                    // For demonstration, I'll use existing Role's Description to store user-specific permissions
                    var userSpecificRole = new ApplicationRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"User_{userId}_CustomPermissions",
                        NormalizedName = $"USER_{userId}_CUSTOMPERMISSIONS",
                        Description = permissionsJson,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId,
                        LastModifiedAt = DateTime.UtcNow,
                        LastModifiedBy = currentUserId
                    };

                    // Create or update the role
                    var existingRole = await _roleManager.FindByNameAsync(userSpecificRole.Name);
                    if (existingRole != null)
                    {
                        existingRole.Description = permissionsJson;
                        existingRole.LastModifiedAt = DateTime.UtcNow;
                        existingRole.LastModifiedBy = currentUserId;

                        await _roleManager.UpdateAsync(existingRole);
                    }
                    else
                    {
                        await _roleManager.CreateAsync(userSpecificRole);
                    }

                    // Also assign this special role to the user to track custom permissions
                    await _userManager.AddToRoleAsync(user, userSpecificRole.Name);

                    await _repository.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Transaction failed, all changes were rolled back", ex);
            }

            // Create audit log for successful assignment
            await CreateAuditLog(
                "Role Assigned",
                $"CorrelationId: {correlationId} - Assigned role '{role.Name}' to user {user.Email} ({user.FirstName} {user.LastName})",
                "Role Management"
            );

            // Get updated roles
            var updatedRoles = await _userManager.GetRolesAsync(user);

            // Prepare response
            var response = new AssignRoleResponseDto
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserFullName = $"{user.FirstName} {user.LastName}",
                AssignedRole = role.Name,
                PreviousRoles = currentRoles.ToList(),
                CurrentRoles = updatedRoles.ToList(),
                AssignedPermissions = permissions?.ToList() ?? new List<PermissionDto>()
            };

            return ResponseFactory.Success(response, "Role and permissions assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role and permissions");
            await CreateAuditLog(
                "Role Assignment Failed",
                $"CorrelationId: {correlationId} - Error: {ex.Message}",
                "Role Management"
            );
            return ResponseFactory.Fail<AssignRoleResponseDto>(ex, "An unexpected error occurred");
        }
    }

    // Helper method to determine the user type
    /*private string DetermineUserType(ApplicationUser user)
    {
        if (user.Admin != null) return "Admin";
        if (user.Chairman != null) return "Chairman";
        if (user.Trader != null) return "Trader";
        if (user.Vendor != null) return "Vendor";
        if (user.Customer != null) return "Customer";
        if (user.Caretaker != null) return "Caretaker";
        if (user.GoodBoy != null) return "GoodBoy";
        if (user.AssistCenterOfficer != null) return "AssistCenterOfficer";

        return "User"; // Default if no specific entity is associated
    }*/

    // Helper method to determine user type and map properties accordingly
    private async Task<UserResponseDto> MapToUserResponseDto(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var userDto = new UserResponseDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToList(),
            UserType = DetermineUserType(user, roles)
        };

        // Additional user details based on type
        if (user.Admin != null)
        {
            userDto.Department = user.Admin.Department;
            userDto.Position = user.Admin.Position;
        }
        else if (user.Chairman != null)
        {
            userDto.LocalGovernmentName = user.LocalGovernment?.Name;
        }
        else if (user.Trader != null)
        {
            userDto.MarketName = user.Trader.Market?.MarketName;
        }

        return userDto;
    }

    // Helper method to determine user type
    private string DetermineUserType(ApplicationUser user, IList<string> roles)
    {
        if (user.Admin != null) return UserRoles.Admin;
        if (user.Chairman != null) return UserRoles.Chairman;
        if (user.Trader != null) return UserRoles.Trader;
        if (user.Vendor != null) return UserRoles.Vendor;
        if (user.Customer != null) return UserRoles.Customer;
        if (user.Caretaker != null) return UserRoles.Caretaker;
        if (user.GoodBoy != null) return UserRoles.Goodboy;
        if (user.AssistCenterOfficer != null) return UserRoles.AssistOfficer;

        // Fallback to the first role if no specific entity is associated
        return roles.FirstOrDefault() ?? "User";
    }

    public async Task<BaseResponse<RoleResponseDto>> UpdateRole(string roleId, UpdateRoleRequestDto updateRoleDto)
    {
        try
        {
            // Check current user's permission
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasRoleManagementAccess)
            {
                await CreateAuditLog(
                    "Role Update Denied",
                    $"Unauthorized role update attempt by user: {userId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to update roles");
            }

            // Validate request
            var validationResult = await _updateRoleValidator.ValidateAsync(updateRoleDto);
            if (!validationResult.IsValid)
            {
                await CreateAuditLog(
                    "Role Update Failed",
                    $"Validation failed for role update ID: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new ValidationException(validationResult.Errors),
                    "Validation failed");
            }

            var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: true);
            if (role == null)
            {
                await CreateAuditLog(
                    "Role Update Failed",
                    $"Role not found for ID: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<RoleResponseDto>(
                    new NotFoundException("Role not found"),
                    "Role not found");
            }

            // Track all changes for audit log
            var changes = new List<string>();

            // Debug logging
            _logger.LogInformation("Current Role Name: {Name}", role.Name);
            _logger.LogInformation("New Role Name: {Name}", updateRoleDto.Name);

            // Check for duplicate name only if name is changing
            if (role.Name != updateRoleDto.Name)
            {
                if (await _repository.AdminRepository.RoleExistsAsync(updateRoleDto.Name, roleId))
                {
                    await CreateAuditLog(
                        "Role Update Failed",
                        $"Role name already exists: {updateRoleDto.Name}",
                        "Role Management"
                    );
                    return ResponseFactory.Fail<RoleResponseDto>("Role name already exists");
                }

                changes.Add($"Name: {role.Name} → {updateRoleDto.Name}");
                role.Name = updateRoleDto.Name;
                role.NormalizedName = updateRoleDto.Name.ToUpper();
            }

            var originalPermissions = role.Permissions.Select(p => p.Name).ToList();
            var addedPermissions = updateRoleDto.Permissions.Except(originalPermissions).ToList();
            var removedPermissions = originalPermissions.Except(updateRoleDto.Permissions).ToList();

            /*            if (addedPermissions.Any() || removedPermissions.Any())
                        {
                            if (addedPermissions.Any())
                                changes.Add($"Added permissions: {string.Join(", ", addedPermissions)}");
                            if (removedPermissions.Any())
                                changes.Add($"Removed permissions: {string.Join(", ", removedPermissions)}");

                            // First, remove permissions that should be removed
                            var permissionsToRemove = role.Permissions
                                .Where(p => removedPermissions.Contains(p.Name))
                                .ToList();

                            foreach (var permission in permissionsToRemove)
                            {
                                _dbContext.RolePermissions.Remove(permission);
                            }

                            // Then add new permissions
                            var permissionsToAdd = addedPermissions.Select(p => new RolePermission
                            {
                                Id = Guid.NewGuid().ToString(),
                                RoleId = roleId,
                                Name = p,
                                IsGranted = true
                            }).ToList();

                            await _dbContext.RolePermissions.AddRangeAsync(permissionsToAdd);

                            // Save changes immediately to handle the permissions
                            await _dbContext.SaveChangesAsync();

                            // Refresh the role's permissions
                            role.Permissions = role.Permissions
                                .Where(p => !removedPermissions.Contains(p.Name))
                                .Concat(permissionsToAdd)
                                .ToList();
                        }*/

            if (addedPermissions.Any() || removedPermissions.Any())
            {
                if (addedPermissions.Any())
                    changes.Add($"Added permissions: {string.Join(", ", addedPermissions)}");
                if (removedPermissions.Any())
                    changes.Add($"Removed permissions: {string.Join(", ", removedPermissions)}");

                // Remove old permissions
                var permissionsToRemove = role.Permissions
                    .Where(p => removedPermissions.Contains(p.Name))
                    .ToList();

                _repository.AdminRepository.DeleteRolePermissions(permissionsToRemove);

                // Add new permissions
                var permissionsToAdd = addedPermissions.Select(p => new RolePermission
                {
                    Id = Guid.NewGuid().ToString(),
                    RoleId = roleId,
                    Name = p,
                    IsGranted = true
                }).ToList();

                await _repository.AdminRepository.AddRolePermissionsAsync(permissionsToAdd);

                // Save changes to handle permissions
                await _repository.SaveChangesAsync();

                // Update the role's permissions collection
                role.Permissions = role.Permissions
                    .Where(p => !removedPermissions.Contains(p.Name))
                    .Concat(permissionsToAdd)
                    .ToList();
            }

            // Update other role properties
            /*  if (role.Name != updateRoleDto.Name)
              {
                  if (await _repository.AdminRepository.RoleExistsAsync(updateRoleDto.Name, roleId))
                  {
                      await CreateAuditLog(
                          "Role Update Failed",
                          $"Role name already exists: {updateRoleDto.Name}",
                          "Role Management"
                      );
                      return ResponseFactory.Fail<RoleResponseDto>("Role name already exists");
                  }

                  changes.Add($"Name: {role.Name} → {updateRoleDto.Name}");
                  role.Name = updateRoleDto.Name;
                  role.NormalizedName = updateRoleDto.Name.ToUpper();
              }*/

            // Update description if changed
            if (role.Description != updateRoleDto.Description)
            {
                changes.Add($"Description: {role.Description} → {updateRoleDto.Description}");
                role.Description = updateRoleDto.Description;
            }

            // Update IsActive status if changed
            if (role.IsActive != updateRoleDto.IsActive)
            {
                changes.Add($"Active Status: {role.IsActive} → {updateRoleDto.IsActive}");
                role.IsActive = updateRoleDto.IsActive;
            }

            if (changes.Any())
            {
                role.LastModifiedBy = userId;
                role.LastModifiedAt = DateTime.UtcNow;

                _repository.AdminRepository.UpdateRole(role);
                await _repository.SaveChangesAsync();

                await CreateAuditLog(
                    "Updated Role",
                    $"Updated role {role.Name}. Changes: {string.Join("; ", changes)}",
                    "Role Management"
                );
            }

            var responseDto = _mapper.Map<RoleResponseDto>(role);
            return ResponseFactory.Success(responseDto, changes.Any()
                ? "Role updated successfully"
                : "No changes were made to role");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role");
            return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
        }
    }
    /* public async Task<BaseResponse<RoleResponseDto>> UpdateRole(string roleId, UpdateRoleRequestDto updateRoleDto)
     {
         try
         {
             var validationResult = await _updateRoleValidator.ValidateAsync(updateRoleDto);
             if (!validationResult.IsValid)
             {
                 await CreateAuditLog(
                     "Role Update Failed",
                     $"Validation failed for role update ID: {roleId}",
                     "Role Management"
                 );
                 return ResponseFactory.Fail<RoleResponseDto>(
                     new ValidationException(validationResult.Errors),
                     "Validation failed");
             }

             var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: true);
             if (role == null)
             {
                 await CreateAuditLog(
                     "Role Update Failed",
                     $"Role not found for ID: {roleId}",
                     "Role Management"
                 );
                 return ResponseFactory.Fail<RoleResponseDto>(
                     new NotFoundException("Role not found"),
                     "Role not found");
             }

             // Check for duplicate name
             if (await _repository.AdminRepository.RoleExistsAsync(updateRoleDto.Name, roleId))
             {
                 await CreateAuditLog(
                     "Role Update Failed",
                     $"Role name already exists: {updateRoleDto.Name}",
                     "Role Management"
                 );
                 return ResponseFactory.Fail<RoleResponseDto>("Role name already exists");
             }

             // Track changes for audit
             var originalPermissions = role.Permissions.Select(p => p.Name).ToList();
             var addedPermissions = updateRoleDto.Permissions.Except(originalPermissions).ToList();
             var removedPermissions = originalPermissions.Except(updateRoleDto.Permissions).ToList();

             // Update role properties
             role.Name = updateRoleDto.Name;
             role.NormalizedName = updateRoleDto.Name.ToUpper();
             role.LastModifiedBy = _currentUser.GetUserId();
             role.LastModifiedAt = DateTime.UtcNow;

             // Update permissions
             role.Permissions.Clear();
             role.Permissions = updateRoleDto.Permissions.Select(p => new RolePermission
             {
                 Id = Guid.NewGuid().ToString(),
                 Name = p,
                 RoleId = roleId,  // Add this line to set the RoleId
                 IsGranted = true
             }).ToList();

             _repository.AdminRepository.UpdateRole(role);
             await _repository.SaveChangesAsync();

             var changes = new List<string>();
             if (addedPermissions.Any())
                 changes.Add($"Added permissions: {string.Join(", ", addedPermissions)}");
             if (removedPermissions.Any())
                 changes.Add($"Removed permissions: {string.Join(", ", removedPermissions)}");

             await CreateAuditLog(
                 "Updated Role",
                 $"Updated role {role.Name}. Changes: {string.Join("; ", changes)}",
                 "Role Management"
             );

             var responseDto = _mapper.Map<RoleResponseDto>(role);
             return ResponseFactory.Success(responseDto, "Role updated successfully");
         }
         catch (Exception ex)
         {
             _logger.LogError(ex, "Error updating role");
             return ResponseFactory.Fail<RoleResponseDto>(ex, "An unexpected error occurred");
         }
     }
 */
    public async Task<BaseResponse<bool>> DeleteRole(string roleId)
    {
        try
        {
            var role = await _repository.AdminRepository.GetRoleByIdAsync(roleId, trackChanges: true);
            if (role == null)
            {
                await CreateAuditLog(
                    "Role Deletion Failed",
                    $"Role not found for ID: {roleId}",
                    "Role Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Role not found"),
                    "Role not found");
            }

            // Check if trying to delete Admin role
            if (role.Name.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                await CreateAuditLog(
                    "Protected Role Deletion Attempted",
                    $"Attempted to delete protected Admin role (ID: {roleId})",
                    "Role Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Protected role"),
                    "The Admin role cannot be deleted as it is a protected system role");
            }

            // Check if role is in use
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                await CreateAuditLog(
                    "Role Deletion Failed",
                    $"Role {role.Name} is still assigned to users",
                    "Role Management"
                );
                return ResponseFactory.Fail<bool>("Cannot delete role as it is assigned to users");
            }

            // Instead of Clear(), remove each permission directly
            if (role.Permissions != null && role.Permissions.Any())
            {
                foreach (var permission in role.Permissions.ToList())
                {
                    _repository.AdminRepository.DeleteRolePermission(permission);
                }
                await _repository.SaveChangesAsync();
            }

            _repository.AdminRepository.DeleteRole(role);
            await _repository.SaveChangesAsync();

            await CreateAuditLog(
                "Deleted Role",
                $"Deleted role {role.Name}",
                "Role Management"
            );

            return ResponseFactory.Success(true, "Role deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    public async Task<BaseResponse<TeamMemberResponseDto>> CreateTeamMember(CreateTeamMemberRequestDto requestDto)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Creation Denied",
                    $"Unauthorized team member creation attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to create team members");
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(requestDto.EmailAddress);
            if (existingUser != null)
            {
                await CreateAuditLog(
                    "Team Member Creation Failed",
                    $"Email already exists: {requestDto.EmailAddress}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>("Email address already exists");
            }

            // Generate a new unique ID for the team member
            var newUserId = Guid.NewGuid().ToString();
            var user = new ApplicationUser
            {
                Id = newUserId,
                UserName = requestDto.EmailAddress,
                Email = requestDto.EmailAddress,
                PhoneNumber = requestDto.PhoneNumber,
                NormalizedUserName = requestDto.EmailAddress,
                NormalizedEmail = requestDto.EmailAddress,
                FirstName = requestDto.FullName.Split(' ')[0],
                LastName = requestDto.FullName.Contains(" ") ?
                    string.Join(" ", requestDto.FullName.Split(' ').Skip(1)) : "",
                ProfileImageUrl = "",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            // Generate a default password
            var defaultPassword = GenerateDefaultPassword(requestDto.FullName);
            var createUserResult = await _userManager.CreateAsync(user, defaultPassword);

            if (!createUserResult.Succeeded)
            {
                await CreateAuditLog(
                    "Team Member Creation Failed",
                    $"Failed to create user account for: {requestDto.EmailAddress}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new ValidationException(createUserResult.Errors.Select(e =>
                        new ValidationFailure(e.Code, e.Description))),
                    "Failed to create user account");
            }

            // Add to team member role
            await _userManager.AddToRoleAsync(user, UserRoles.TeamMember);

            // Create Admin entity with team member privileges
            var admin = new Admin
            {
                UserId = user.Id,
                AdminLevel = requestDto.AdminLevel ?? "Team Member",
                Department = requestDto.Department ?? "General",
                Position = requestDto.Position ?? "Team Member",
                HasDashboardAccess = requestDto.HasDashboardAccess ?? false,
                HasRoleManagementAccess = requestDto.HasRoleManagementAccess ?? false,
                HasTeamManagementAccess = requestDto.HasTeamManagementAccess ?? false,
                HasAuditLogAccess = requestDto.HasAuditLogAccess ?? false,
                HasAdvertManagementAccess = requestDto.HasAdvertManagementAccess ?? false,
                RegisteredLGAs = 0,
                ActiveChairmen = 0,
                TotalRevenue = 0,
                StatsLastUpdatedAt = DateTime.UtcNow
            };

            _repository.AdminRepository.CreateAdmin(admin);
            await _repository.SaveChangesAsync();

            await CreateAuditLog(
                "Created Team Member",
                $"Created team member account for {user.Email} ({requestDto.FullName})",
                "Team Management"
            );

            var responseDto = new TeamMemberResponseDto
            {
                Id = user.Id,
                FullName = requestDto.FullName,
                PhoneNumber = requestDto.PhoneNumber,
                EmailAddress = requestDto.EmailAddress,
                DateAdded = user.CreatedAt,
                DefaultPassword = defaultPassword
            };

            return ResponseFactory.Success(responseDto, "Team member created successfully. Please note down the default password.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team member");
            return ResponseFactory.Fail<TeamMemberResponseDto>(ex, "An unexpected error occurred");
        }
    }

    private string GenerateDefaultPassword(string fullName)
    {
        // Handle null or empty fullName
        if (string.IsNullOrWhiteSpace(fullName))
        {
            fullName = "User";
        }

        var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : "User";
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

        var random = new Random();
        var randomNumbers = random.Next(100, 999).ToString(); // Generate a 3-digit random number

        // Special characters pool
        var specialChars = "!@#$%^&*(),.?\":{}|<>";
        var specialChar = specialChars[random.Next(specialChars.Length)];

        // Generate random uppercase letter
        var uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var uppercaseLetter = uppercaseLetters[random.Next(uppercaseLetters.Length)];

        // Generate random lowercase letter - ensure we have at least one
        var lowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        var lowercaseLetter = lowercaseLetters[random.Next(lowercaseLetters.Length)];

        // Process name parts - ensure they're properly formatted
        string firstNameProcessed = "";
        if (firstName.Length > 0)
        {
            firstNameProcessed = char.ToUpper(firstName[0]) +
                (firstName.Length > 1 ? firstName.Substring(1).ToLower() : "");
        }

        string lastNameProcessed = "";
        if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
        {
            lastNameProcessed = char.ToUpper(lastName[0]) +
                (lastName.Length > 1 ? lastName.Substring(1).ToLower() : "");
        }

        // Combine all elements ensuring we have all required character types
        var passwordParts = new List<string>
    {
        firstNameProcessed,
        lastNameProcessed,
        randomNumbers,          // Ensures numbers
        uppercaseLetter.ToString(), // Ensures uppercase
        lowercaseLetter.ToString(), // Ensures lowercase
        specialChar.ToString()  // Ensures special character
    };

        // Remove any empty parts
        passwordParts.RemoveAll(string.IsNullOrEmpty);

        // Shuffle the parts for better security
        ShuffleList(passwordParts, random);

        var password = string.Join("", passwordParts);

        // Ensure minimum length of 8 characters
        if (password.Length < 8)
        {
            var additionalChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
            while (password.Length < 8)
            {
                password += additionalChars[random.Next(additionalChars.Length)];
            }
        }

        // Final verification to ensure the password meets all requirements
        if (!ContainsUppercase(password)) password += uppercaseLetters[random.Next(uppercaseLetters.Length)];
        if (!ContainsLowercase(password)) password += lowercaseLetters[random.Next(lowercaseLetters.Length)];
        if (!ContainsNumber(password)) password += random.Next(0, 9).ToString();
        if (!ContainsSpecialChar(password)) password += specialChars[random.Next(specialChars.Length)];

        return password;
    }

    // Helper method to shuffle a list
    private void ShuffleList<T>(List<T> list, Random random)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Helper methods to verify password requirements
    private bool ContainsUppercase(string password)
    {
        return password.Any(char.IsUpper);
    }

    private bool ContainsLowercase(string password)
    {
        return password.Any(char.IsLower);
    }

    private bool ContainsNumber(string password)
    {
        return password.Any(char.IsDigit);
    }

    private bool ContainsSpecialChar(string password)
    {
        return password.Any(c => "!@#$%^&*(),.?\":{}|<>".Contains(c));
    }
    /*  public async Task<BaseResponse<TeamMemberResponseDto>> CreateTeamMember(CreateTeamMemberRequestDto requestDto)
      {
          try
          {
              var userId = _currentUser.GetUserId();
              var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

              if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
              {
                  await CreateAuditLog(
                      "Team Member Creation Denied",
                      $"Unauthorized team member creation attempt by user: {userId}",
                      "Team Management"
                  );
                  return ResponseFactory.Fail<TeamMemberResponseDto>(
                      new UnauthorizedException("Access denied"),
                      "You don't have permission to create team members");
              }

              // Check if email already exists
              var existingUser = await _userManager.FindByEmailAsync(requestDto.EmailAddress);
              if (existingUser != null)
              {
                  await CreateAuditLog(
                      "Team Member Creation Failed",
                      $"Email already exists: {requestDto.EmailAddress}",
                      "Team Management"
                  );
                  return ResponseFactory.Fail<TeamMemberResponseDto>("Email address already exists");
              }

              // Generate a new unique ID for the team member
              var newUserId = Guid.NewGuid().ToString();

              var user = new ApplicationUser
              {
                  Id = newUserId,                       // Use the new unique ID instead of admin's ID
                  UserName = requestDto.EmailAddress,
                  Email = requestDto.EmailAddress,
                  PhoneNumber = requestDto.PhoneNumber,
                  NormalizedUserName = requestDto.EmailAddress,
                  NormalizedEmail = requestDto.EmailAddress,
                  FirstName = requestDto.FullName.Split(' ')[0],
                  LastName = requestDto.FullName.Contains(" ") ?
                      string.Join(" ", requestDto.FullName.Split(' ').Skip(1)) : "",
                  ProfileImageUrl = "",
                  IsActive = true,
                  EmailConfirmed = true,
                  CreatedAt = DateTime.UtcNow
              };

              var createUserResult = await _userManager.CreateAsync(user);
              if (!createUserResult.Succeeded)
              {
                  await CreateAuditLog(
                      "Team Member Creation Failed",
                      $"Failed to create user account for: {requestDto.EmailAddress}",
                      "Team Management"
                  );
                  return ResponseFactory.Fail<TeamMemberResponseDto>(
                      new ValidationException(createUserResult.Errors.Select(e =>
                          new ValidationFailure(e.Code, e.Description))),
                      "Failed to create user account");
              }

              // Add to team member role
              await _userManager.AddToRoleAsync(user, UserRoles.TeamMember);

              await CreateAuditLog(
                  "Created Team Member",
                  $"Created team member account for {user.Email} ({requestDto.FullName})",
                  "Team Management"
              );

              var responseDto = new TeamMemberResponseDto
              {
                  Id = user.Id,
                  FullName = requestDto.FullName,
                  PhoneNumber = requestDto.PhoneNumber,
                  EmailAddress = requestDto.EmailAddress,
                  DateAdded = user.CreatedAt
              };

              return ResponseFactory.Success(responseDto, "Team member created successfully");
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Error creating team member");
              return ResponseFactory.Fail<TeamMemberResponseDto>(ex, "An unexpected error occurred");
          }
      }*/
    public async Task<BaseResponse<TeamMemberResponseDto>> UpdateTeamMember(
       string memberId,
       UpdateTeamMemberRequestDto requestDto)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Update Denied",
                    $"Unauthorized team member update attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to update team members");
            }

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Team Member Update Failed",
                    $"Team member not found with ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new NotFoundException("Team member not found"),
                    "Team member not found");
            }

            // Track changes for audit log
            var changes = new List<string>();

            // Debug logging
            _logger.LogInformation("Current Values - FirstName: {FirstName}, LastName: {LastName}",
                user.FirstName, user.LastName);
            _logger.LogInformation("New FullName: {FullName}", requestDto.FullName);

            // Update name if provided
            if (!string.IsNullOrWhiteSpace(requestDto.FullName))
            {
                var currentFullName = $"{user.FirstName} {user.LastName}".Trim();
                var newFullName = requestDto.FullName.Trim();

                if (currentFullName != newFullName)
                {
                    changes.Add($"Name: {currentFullName} → {newFullName}");

                    var nameParts = newFullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    user.FirstName = nameParts[0];
                    user.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
                }
            }

            // Update email if provided
            if (!string.IsNullOrWhiteSpace(requestDto.EmailAddress) && user.Email != requestDto.EmailAddress)
            {
                changes.Add($"Email: {user.Email} → {requestDto.EmailAddress}");
                user.Email = requestDto.EmailAddress;
                user.UserName = requestDto.EmailAddress;
                user.EmailConfirmed = true;
            }

            // Update phone if provided
            if (!string.IsNullOrWhiteSpace(requestDto.PhoneNumber) && user.PhoneNumber != requestDto.PhoneNumber)
            {
                changes.Add($"Phone: {user.PhoneNumber} → {requestDto.PhoneNumber}");
                user.PhoneNumber = requestDto.PhoneNumber;
            }

            // Only proceed with update if there are changes
            if (changes.Any())
            {
                _logger.LogInformation("Applying changes: {@Changes}", changes);

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await CreateAuditLog(
                        "Team Member Update Failed",
                        $"Failed to update user properties for ID: {memberId}",
                        "Team Management"
                    );
                    return ResponseFactory.Fail<TeamMemberResponseDto>(
                        new ValidationException(updateResult.Errors.Select(e =>
                            new ValidationFailure(e.Code, e.Description))),
                        "Failed to update team member");
                }

                await CreateAuditLog(
                    "Updated Team Member",
                    $"Updated team member {user.Email}. Changes: {string.Join(", ", changes)}",
                    "Team Management"
                );
            }
            else
            {
                _logger.LogInformation("No changes detected for user {UserId}", memberId);
            }

            var responseDto = new TeamMemberResponseDto
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                EmailAddress = user.Email,
                DateAdded = user.CreatedAt
            };

            return ResponseFactory.Success(responseDto, changes.Any()
                ? "Team member updated successfully"
                : "No changes were made to team member");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team member");
            return ResponseFactory.Fail<TeamMemberResponseDto>(ex, "An unexpected error occurred");
        }
    }
    public async Task<BaseResponse<TeamMemberResponseDto>> GetTeamMemberById(string memberId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Access Denied",
                    $"Unauthorized team member access attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to view team members");
            }

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Team Member Lookup Failed",
                    $"Team member not found with ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<TeamMemberResponseDto>(
                    new NotFoundException("Team member not found"),
                    "Team member not found");
            }

            var responseDto = new TeamMemberResponseDto
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber,
                EmailAddress = user.Email,
                DateAdded = user.CreatedAt
            };

            await CreateAuditLog(
                "Team Member Lookup",
                $"Retrieved team member details for ID: {memberId}",
                "Team Management"
            );

            return ResponseFactory.Success(responseDto, "Team member retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team member");
            return ResponseFactory.Fail<TeamMemberResponseDto>(ex, "An unexpected error occurred");
        }
    }

    /*  public async Task<BaseResponse<bool>> DeleteTeamMember(string memberId)
      {
          try
          {
              var userId = _currentUser.GetUserId();
              var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
              if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
              {
                  await CreateAuditLog(
                      "Team Member Deletion Denied",
                      $"Unauthorized team member deletion attempt by user: {userId}",
                      "Team Management"
                  );
                  return ResponseFactory.Fail<bool>(
                      new UnauthorizedException("Access denied"),
                      "You don't have permission to delete team members");
              }

              // Use DbContext directly to avoid UserManager tracking conflicts
              var user = await _dbContext.Users
                  .FirstOrDefaultAsync(u => u.Id == memberId);

              if (user == null)
              {
                  await CreateAuditLog(
                      "Team Member Deletion Failed",
                      $"Team member not found with ID: {memberId}",
                      "Team Management"
                  );
                  return ResponseFactory.Fail<bool>(
                      new NotFoundException("Team member not found"),
                      "Team member not found");
              }

              // Store user info for audit log before update
              var userEmail = user.Email;
              var userFullName = $"{user.FirstName} {user.LastName}";

              // Soft delete: deactivate the user
              user.IsActive = false;
              user.IsBlocked = true; // Optional: also block the user

              // Save changes directly through DbContext
              var rowsAffected = await _dbContext.SaveChangesAsync();

              if (rowsAffected == 0)
              {
                  await CreateAuditLog(
                      "Team Member Deletion Failed",
                      $"Failed to deactivate user account for ID: {memberId}",
                      "Team Management"
                  );
                  return ResponseFactory.Fail<bool>(
                      new ValidationException(new[] { new ValidationFailure("Update", "No changes were saved") }),
                      "Failed to delete team member");
              }

              await CreateAuditLog(
                  "Deleted Team Member",
                  $"Deactivated team member account for {userEmail} ({userFullName})",
                  "Team Management"
              );

              return ResponseFactory.Success(true, "Team member deleted successfully");
          }
          catch (Exception ex)
          {
              _logger.LogError(ex, "Error deleting team member");
              return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
          }
      }*/

    public async Task<BaseResponse<bool>> DeleteTeamMember(string memberId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);
            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Deletion Denied",
                    $"Unauthorized team member deletion attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to delete team members");
            }

            // Use DbContext directly to avoid UserManager tracking conflicts
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == memberId);

            if (user == null)
            {
                await CreateAuditLog(
                    "Team Member Deletion Failed",
                    $"Team member not found with ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Team member not found"),
                    "Team member not found");
            }

            // Store user info for audit log before deletion
            var userEmail = user.Email;
            var userFullName = $"{user.FirstName} {user.LastName}";

            // Hard delete: Remove related entities first to avoid foreign key conflicts

            // 1. Delete Admin record if exists
            var adminRecord = await _dbContext.Admins
                .FirstOrDefaultAsync(a => a.UserId == memberId);
            if (adminRecord != null)
            {
                _dbContext.Admins.Remove(adminRecord);
            }

            // 2. Delete other related entities based on user type
            // Check and delete Chairman
            var chairman = await _dbContext.Chairmen
                .FirstOrDefaultAsync(c => c.UserId == memberId);
            if (chairman != null)
            {
                _dbContext.Chairmen.Remove(chairman);
            }

            // Check and delete Trader
            var trader = await _dbContext.Traders
                .FirstOrDefaultAsync(t => t.UserId == memberId);
            if (trader != null)
            {
                _dbContext.Traders.Remove(trader);
            }

            // Check and delete Vendor
            var vendor = await _dbContext.Vendors
                .FirstOrDefaultAsync(v => v.UserId == memberId);
            if (vendor != null)
            {
                _dbContext.Vendors.Remove(vendor);
            }

            // Check and delete Customer
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.UserId == memberId);
            if (customer != null)
            {
                _dbContext.Customers.Remove(customer);
            }

            // Check and delete Caretaker
            var caretaker = await _dbContext.Caretakers
                .FirstOrDefaultAsync(c => c.UserId == memberId);
            if (caretaker != null)
            {
                _dbContext.Caretakers.Remove(caretaker);
            }

            // Check and delete GoodBoy
            var goodBoy = await _dbContext.GoodBoys
                .FirstOrDefaultAsync(g => g.UserId == memberId);
            if (goodBoy != null)
            {
                _dbContext.GoodBoys.Remove(goodBoy);
            }

            // Check and delete AssistCenterOfficer
            var assistOfficer = await _dbContext.AssistCenterOfficers
                .FirstOrDefaultAsync(a => a.UserId == memberId);
            if (assistOfficer != null)
            {
                _dbContext.AssistCenterOfficers.Remove(assistOfficer);
            }

            // 3. Remove any audit logs related to this user (optional - you might want to keep these)
            // var auditLogs = await _dbContext.AuditLogs
            //     .Where(a => a.AdminId == memberId)
            //     .ToListAsync();
            // if (auditLogs.Any())
            // {
            //     _dbContext.AuditLogs.RemoveRange(auditLogs);
            // }

            // 4. Finally, delete the ApplicationUser
            _dbContext.Users.Remove(user);

            // Save all changes
            var rowsAffected = await _dbContext.SaveChangesAsync();

            if (rowsAffected == 0)
            {
                await CreateAuditLog(
                    "Team Member Deletion Failed",
                    $"Failed to delete user account for ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException(new[] { new ValidationFailure("Delete", "No changes were saved") }),
                    "Failed to delete team member");
            }

            await CreateAuditLog(
                "Deleted Team Member",
                $"Permanently deleted team member account for {userEmail} ({userFullName})",
                "Team Management"
            );

            return ResponseFactory.Success(true, "Team member deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team member");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }

    /*public async Task<BaseResponse<bool>> DeleteTeamMember(string memberId)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Member Deletion Denied",
                    $"Unauthorized team member deletion attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to delete team members");
            }

            var user = await _userManager.FindByIdAsync(memberId);
            if (user == null)
            {
                await CreateAuditLog(
                    "Team Member Deletion Failed",
                    $"Team member not found with ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new NotFoundException("Team member not found"),
                    "Team member not found");
            }

            // Instead of hard delete, deactivate the user
            user.IsActive = false;
            var updateResult = await _userManager.DeleteAsync(user);
            if (!updateResult.Succeeded)
            {
                await CreateAuditLog(
                    "Team Member Deletion Failed",
                    $"Failed to deactivate user account for ID: {memberId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<bool>(
                    new ValidationException(updateResult.Errors.Select(e =>
                        new ValidationFailure(e.Code, e.Description))),
                    "Failed to delete team member");
            }

            await CreateAuditLog(
                "Deleted Team Member",
                $"Deactivated team member account for {user.Email} ({user.FirstName} {user.LastName})",
                "Team Management"
            );

            return ResponseFactory.Success(true, "Team member deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team member");
            return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
        }
    }*/
    public async Task<BaseResponse<PaginatorDto<IEnumerable<TeamMemberResponseDto>>>> GetTeamMembers(
     TeamMemberFilterRequestDto filterDto,
     PaginationFilter paginationFilter)
    {
        try
        {
            var userId = _currentUser.GetUserId();
            var currentAdmin = await _repository.AdminRepository.GetAdminByIdAsync(userId, trackChanges: false);

            if (currentAdmin == null || !currentAdmin.HasTeamManagementAccess)
            {
                await CreateAuditLog(
                    "Team Members List Access Denied",
                    $"Unauthorized team members list access attempt by user: {userId}",
                    "Team Management"
                );
                return ResponseFactory.Fail<PaginatorDto<IEnumerable<TeamMemberResponseDto>>>(
                    new UnauthorizedException("Access denied"),
                    "You don't have permission to view team members");
            }

            // Get all team member user IDs from UserRoles table
            var teamMemberIds = await _userManager.GetUsersInRoleAsync(UserRoles.TeamMember);
            var teamMemberIdList = teamMemberIds.Select(u => u.Id).ToList();

            // Query users with those IDs
            var query = _userManager.Users.Where(u => teamMemberIdList.Contains(u.Id));

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(filterDto?.SearchTerm))
            {
                var searchTerm = filterDto.SearchTerm.Trim().ToLower();
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
            }

            // Only include active members
            query = query.Where(u => u.IsActive);

            // Apply ordering
            query = query.OrderByDescending(u => u.CreatedAt);

            // Apply pagination
            var paginatedMembers = await query.Paginate(paginationFilter);

            // Map to response DTOs
            var memberDtos = paginatedMembers.PageItems.Select(user => new TeamMemberResponseDto
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                EmailAddress = user.Email,
                DateAdded = user.CreatedAt
            });

            // Create paginated result
            var result = new PaginatorDto<IEnumerable<TeamMemberResponseDto>>
            {
                PageItems = memberDtos,
                PageSize = paginatedMembers.PageSize,
                CurrentPage = paginatedMembers.CurrentPage,
                NumberOfPages = paginatedMembers.NumberOfPages,
                TotalItems = paginatedMembers.TotalItems,
            };

            await CreateAuditLog(
                "Team Members List Retrieved",
                $"Retrieved team members list - Page {paginationFilter.PageNumber}, " +
                $"Size {paginationFilter.PageSize}, Search: {filterDto?.SearchTerm ?? "none"}",
                "Team Management"
            );

            return ResponseFactory.Success(result, "Team members retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team members list: {Error}", ex.Message);
            return ResponseFactory.Fail<PaginatorDto<IEnumerable<TeamMemberResponseDto>>>(
                ex, "An unexpected error occurred");
        }
    }
}