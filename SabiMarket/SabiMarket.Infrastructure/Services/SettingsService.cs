using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QRCoder;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.IServices;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace SabiMarket.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {

        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IValidator<UpdateProfileDto> _updateProfileValidator;
        private readonly IRepositoryManager _repository;
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public SettingsService(IValidator<ChangePasswordDto> changePasswordValidator, ILogger<AuthenticationService> logger,
            UserManager<ApplicationUser> userManager, IValidator<UpdateProfileDto> updateProfileValidator, IRepositoryManager repository, SignInManager<ApplicationUser> signInManager, ISmsService smsService, IEmailService emailService, IConfiguration configuration)
        {
            _changePasswordValidator = changePasswordValidator;
            _logger = logger;
            _userManager = userManager;
            _updateProfileValidator = updateProfileValidator;
            _repository = repository;
            _signInManager = signInManager;
            _smsService = smsService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<BaseResponse<bool>> ChangePassword(string userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var validationResult = await _changePasswordValidator.ValidateAsync(changePasswordDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<bool>(
                        new FluentValidation.ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "User not found");
                }

                var result = await _userManager.ChangePasswordAsync(user,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(result.Errors.First().Description),
                        "Password change failed");
                }

                return ResponseFactory.Success(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> SendPasswordResetOTP(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                // Validate that at least one identifier is provided
                if (string.IsNullOrWhiteSpace(forgotPasswordDto.EmailAddress) && string.IsNullOrWhiteSpace(forgotPasswordDto.PhoneNumber))
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Either email or phone number must be provided"),
                        "Please provide either an email address or a phone number");
                }

                // Get OTP expiry time from configuration
                int otpExpiryMinutes = _configuration.GetValue<int>("PasswordReset:OtpExpiryMinutes", 5);
                _logger.LogInformation($"Using OTP expiry time of {otpExpiryMinutes} minutes from configuration");

                // Find user by identifier
                ApplicationUser user = null;
                //TODO
                // check if email address exist 

                // First try to find by email if provided
                if (!string.IsNullOrWhiteSpace(forgotPasswordDto.EmailAddress))
                {
                    user = await _userManager.FindByEmailAsync(forgotPasswordDto.EmailAddress);
                }

                // If user not found and phone provided, try by phone
                if (user == null && !string.IsNullOrWhiteSpace(forgotPasswordDto.PhoneNumber))
                {
                    string normalizedPhone = NormalizePhoneNumber(forgotPasswordDto.PhoneNumber);
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);
                }

                if (user == null)
                {
                    // Don't reveal if the user exists or not
                    string message = "If your account exists in our system, an OTP has been sent";
                    return ResponseFactory.Success(true, message);
                }

                // Check environment
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                bool isDevelopment = environment == "Development";

                // Generate OTP: Use static OTP in Dev, Random OTP in Live
                var otp = isDevelopment ? "777777" : GenerateSecureOTP();

                // Store OTP with configurable expiry time
                user.PasswordResetToken = otp;
                user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(otpExpiryMinutes);
                user.PasswordResetMethod = DeliveryMethod.Both;

                // Log securely
                if (isDevelopment)
                {
                    _logger.LogInformation($"Generated OTP: {otp} (Expires in: {otpExpiryMinutes} minutes)");
                }

                await _userManager.UpdateAsync(user);

                // Track if either delivery method succeeds
                bool anyDeliverySuccess = false;

                // Always attempt to send SMS if we have the user's phone number
                if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    string message = $"Your password reset OTP is: {otp}. Valid for {otpExpiryMinutes} minutes.";
                    bool smsSent = await _smsService.SendSMS(user.PhoneNumber, message);

                    if (smsSent)
                    {
                        anyDeliverySuccess = true;
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send SMS to {MaskIdentifier(user.PhoneNumber)}");
                    }
                }

                // Always attempt to send email if we have the user's email
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    if (_emailService != null)
                    {
                        string subject = "Password Reset OTP";
                        string htmlBody = $@"
                <html>
                <body>
                    <h2>Password Reset</h2>
                    <p>You requested a password reset for your account.</p>
                    <p>Your one-time password (OTP) is: <strong>{otp}</strong></p>
                    <p>This OTP is valid for {otpExpiryMinutes} minutes.</p>
                    <p>If you did not request this reset, please ignore this email.</p>
                </body>
                </html>";

                        bool emailSent = await _emailService.SendEmailAsync(user.Email, subject, htmlBody);

                        if (emailSent)
                        {
                            anyDeliverySuccess = true;
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to send email to {MaskIdentifier(user.Email)}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Email service not configured but email delivery attempted");
                    }
                }

                // Check if at least one delivery method succeeded
                if (!anyDeliverySuccess)
                {
                    _logger.LogError("Failed to deliver OTP via any method");
                   /* return ResponseFactory.Fail<bool>(
                        new Exception("Failed to deliver OTP"),
                        "Unable to send verification code. Please try again later.");*/
                }

                return ResponseFactory.Success(true, "Verification code has been sent to your registered contact information");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset OTP");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<bool>> VerifyPasswordResetOTP(VerifyOTPDto verifyOTPDto)
        {
            try
            {
                // Determine if identifier is email or phone
                bool isEmail = verifyOTPDto.EmailAddress!.Contains("@");

                // Find user by identifier
                ApplicationUser user = null;
                if (isEmail)
                {
                    user = await _userManager.FindByEmailAsync(verifyOTPDto.EmailAddress);
                }
                else
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == verifyOTPDto.PhoneNumber);
                }

                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User Email does not exist"),
                        "User does not exist");
                }

                // Verify OTP
                if (user.PasswordResetToken.Trim() != verifyOTPDto.OTP.Trim() )
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Invalid OTP"),
                        "OTP verification failed");
                }
                
                if (user.PasswordResetExpiry < DateTime.UtcNow)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Expired OTP"),
                        "OTP verification failed");
                }

                // Mark as verified (optional, can add this property to user model)
                user.PasswordResetVerified = true;
                await _userManager.UpdateAsync(user);

                // Don't expose the actual OTP in the success message
                return ResponseFactory.Success(true, "OTP verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password reset OTP");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }

        public async Task<BaseResponse<bool>> ResetPasswordAfterOTP(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Validate input
                if (resetPasswordDto == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Invalid request data"),
                        "Password reset failed");
                }

                // Validate password and confirm password match
                if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Passwords do not match"),
                        "The password and confirmation password do not match");
                }

                // Determine if identifier is email or phone
                ApplicationUser user = null;

                // First try to find by email if provided
                if (!string.IsNullOrWhiteSpace(resetPasswordDto.EmailAddress))
                {
                    user = await _userManager.FindByEmailAsync(resetPasswordDto.EmailAddress);
                }
                // Then try by phone if provided and user not found
                else if (!string.IsNullOrWhiteSpace(resetPasswordDto.PhoneNumber))
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == resetPasswordDto.PhoneNumber);
                }
                else
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Missing identifier"),
                        "Either email or phone number must be provided");
                }

                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "Password reset failed");
                }

                // Check if OTP is still valid
                if (string.IsNullOrEmpty(user.PasswordResetToken) || user.PasswordResetExpiry < DateTime.UtcNow)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Invalid or expired reset token"),
                        "Password reset failed");
                }

                // Validate password complexity before attempting to reset
                var passwordValidator = new PasswordValidator<ApplicationUser>();
                var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, user, resetPasswordDto.NewPassword);

                if (!passwordValidationResult.Succeeded)
                {
                    string errorMessages = string.Join(", ", passwordValidationResult.Errors.Select(e => e.Description));
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(errorMessages),
                        "Password doesn't meet the requirements");
                }

                // Reset password without changing SecurityStamp
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(result.Errors.First().Description),
                        "Password reset failed");
                }

                // Clear reset token
                user.PasswordResetToken = null;
                user.PasswordResetExpiry = null;
                user.PasswordResetMethod = null;
                user.PasswordResetVerified = false;
                await _userManager.UpdateAsync(user);

                // Send confirmation notification if email is available
                try
                {
                    if (_emailService != null && !string.IsNullOrEmpty(user.Email))
                    {
                        string subject = "Password Changed Successfully";
                        string htmlBody = $@"
                <html>
                <body>
                    <h2>Password Changed</h2>
                    <p>Your password has been changed successfully.</p>
                    <p>If you did not make this change, please contact support immediately.</p>
                </body>
                </html>";

                        await _emailService.SendEmailAsync(user.Email, subject, htmlBody);
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the password reset if confirmation email fails
                    _logger.LogWarning(ex, "Failed to send password change confirmation");
                }

                return ResponseFactory.Success(true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
       /* public async Task<BaseResponse<bool>> ResetPasswordAfterOTP(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Determine if identifier is email or phone
                bool isEmail = resetPasswordDto.EmailAddress!.Contains("@");

                // Find user by identifier
                ApplicationUser user = null;
                if (isEmail)
                {
                    user = await _userManager.FindByEmailAsync(resetPasswordDto.EmailAddress);
                }
                else
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == resetPasswordDto.PhoneNumber);
                }

                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "Password reset failed");
                }

                // Check if OTP is still valid
                if (string.IsNullOrEmpty(user.PasswordResetToken) || user.PasswordResetExpiry < DateTime.UtcNow)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Invalid or expired reset token"),
                        "Password reset failed");
                }

                // Reset password without changing SecurityStamp
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(result.Errors.First().Description),
                        "Password reset failed");
                }

                // Clear reset token
                user.PasswordResetToken = null;
                user.PasswordResetExpiry = null;
                user.PasswordResetMethod = null;
                user.PasswordResetVerified = false; // If you added this property
                await _userManager.UpdateAsync(user);

                // Send confirmation notification if email is available
                try
                {
                    if (_emailService != null && !string.IsNullOrEmpty(user.Email))
                    {
                        string subject = "Password Changed Successfully";
                        string htmlBody = $@"
                <html>
                <body>
                    <h2>Password Changed</h2>
                    <p>Your password has been changed successfully.</p>
                    <p>If you did not make this change, please contact support immediately.</p>
                </body>
                </html>";

                        await _emailService.SendEmailAsync(user.Email, subject, htmlBody);
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the password reset if confirmation email fails
                    _logger.LogWarning(ex, "Failed to send password change confirmation");
                }

                return ResponseFactory.Success(true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
      */  public async Task<BaseResponse<bool>> LogoutUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(new NotFoundException("User not found"), "User not found");
                }

                // Force logout by invalidating security stamp
                await _userManager.UpdateSecurityStampAsync(user);
                await _signInManager.SignOutAsync();

                return ResponseFactory.Success(true, "User logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return ResponseFactory.Fail<bool>(ex, "An error occurred during logout");
            }
        }


        public async Task<BaseResponse<bool>> UpdateProfile(string userId, UpdateProfileDto updateProfileDto)
        {
            try
            {
                // Validate input
                var validationResult = await _updateProfileValidator.ValidateAsync(updateProfileDto);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Fail<bool>(
                        new ValidationException(validationResult.Errors),
                        "Validation failed");
                }

                // Get user and verify existence
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<bool>(
                        new NotFoundException("User not found"),
                        "User not found");
                }

                // Verify LocalGovernment exists
                var localGovernmentExists = await _repository.LocalGovernmentRepository.LocalGovernmentExist
                    (updateProfileDto.LocalGovernmentId);
                if (!localGovernmentExists)
                {
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException("Invalid Local Government selected"),
                        "Invalid Local Government");
                }

                // Check if email is being changed and verify it's not taken by another user
                if (!string.Equals(user.Email, updateProfileDto.EmailAddress, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _userManager.FindByEmailAsync(updateProfileDto.EmailAddress);
                    if (emailExists != null && emailExists.Id != userId)
                    {
                        return ResponseFactory.Fail<bool>(
                            new BadRequestException("Email address is already in use"),
                            "Email address is already taken");
                    }
                }

                // Update user properties
                var nameParts = updateProfileDto.FullName?.Trim().Split(' ', 2);
                user.FirstName = nameParts?[0] ?? user.FirstName;
                user.LastName = nameParts?.Length > 1 ? nameParts[1] : string.Empty;
                user.Email = updateProfileDto.EmailAddress;
                user.UserName = updateProfileDto.EmailAddress;
                user.PhoneNumber = updateProfileDto.PhoneNumber;
                user.Address = updateProfileDto.Address;
                user.LocalGovernmentId = updateProfileDto.LocalGovernmentId;

                // Update profile image if provided
                if (!string.IsNullOrEmpty(updateProfileDto.ProfileImageUrl))
                {
                    user.ProfileImageUrl = updateProfileDto.ProfileImageUrl;
                }

                // Update user
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return ResponseFactory.Fail<bool>(
                        new BadRequestException(errors),
                        "Profile update failed");
                }

                // Log the successful update
                _logger.LogInformation("Profile updated successfully for user {UserId}", userId);

                return ResponseFactory.Success(true, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {Use[prId}", userId);
                return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
            }
        }
        public async Task<BaseResponse<UserDetailsResponseDto>> GetUserDetails(string userId, string userType)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Fail<UserDetailsResponseDto>(
                        new NotFoundException("User not found"),
                        "User profile not found");
                }

                // Get user's market and other related information
                var marketInfo = await _repository.MarketRepository.GetMarketByUserId(userId, false);

                var traderDetailsResponse = new TraderDetailsResponseDto(); 
                // Base user details
                var baseDetails = new UserDetailsResponseDto
                {
                    Id = user.Id,
                    UserId = userId,
                    FullName = string.Join(" ",
                        new[] { user.FirstName, user.LastName }
                        .Where(x => !string.IsNullOrEmpty(x))),
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    EmailAddress = user.Email ?? string.Empty,
                    Gender = user.Gender,
                    Market = marketInfo?.MarketName ?? string.Empty,
                    LGA = marketInfo?.LocalGovernment?.LGA ?? string.Empty,
                    Address = user.Address ?? string.Empty,
                    DateAdded = user.CreatedAt,
                    IsBlocked = user.IsBlocked,
                    QrCodeData = GenerateQrCodeData(userId)
                };

                // Return specific details based on user type
                switch (userType.ToLower())
                {
                    case "vendor":
                        var vendorDetails = await _repository.VendorRepository.GetVendorDetails(userId);
                        return ResponseFactory.Success(new VendorDetailsResponseDto
                        {
                            // Base details
                            Id = baseDetails.Id,
                            UserId = baseDetails.UserId,
                            FullName = baseDetails.FullName,
                            PhoneNumber = baseDetails.PhoneNumber,
                            EmailAddress = baseDetails.EmailAddress,
                            Gender = baseDetails.Gender,
                            Market = baseDetails.Market,
                            LGA = baseDetails.LGA,
                            Address = baseDetails.Address,
                            DateAdded = baseDetails.DateAdded,
                            IsBlocked = baseDetails.IsBlocked,
                            QrCodeData = baseDetails.QrCodeData,
                            // Vendor-specific details
                            BusinessName = vendorDetails?.BusinessName ?? string.Empty
                        } as UserDetailsResponseDto, "Vendor details retrieved successfully");


                    case "trader":
                        var traderDetails = await _repository.TraderRepository.GetTraderDetails(userId);
                        return ResponseFactory.Success(new UserDetailsResponseDto
                        {
                            Id = baseDetails.Id,
                            UserId = baseDetails.UserId,
                            FullName = baseDetails.FullName,
                            PhoneNumber = baseDetails.PhoneNumber,
                            EmailAddress = baseDetails.EmailAddress,
                            Gender = baseDetails.Gender,
                            Market = baseDetails.Market,
                            LGA = baseDetails.LGA,
                            Address = baseDetails.Address,
                            DateAdded = baseDetails.DateAdded,
                            IsBlocked = baseDetails.IsBlocked,
                            QrCodeData = baseDetails.QrCodeData,
                            TraderDetails = traderDetails == null ? null : new TraderDetailsResponseDto
                            {
                                TraderIdentityNumber = traderDetails.TIN,
                                TraderOccupancy = string.Empty,
                                PaymentFrequency = string.Empty
                            }
                        }, "Trader details retrieved successfully");


                    default:
                        return ResponseFactory.Success(baseDetails, "User details retrieved successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for user {UserId}", userId);
                return ResponseFactory.Fail<UserDetailsResponseDto>(ex, "An unexpected error occurred");
            }
        }

        private string GenerateQrCodeData(string userId)
        {
            try
            {
                // Generate unique QR code data with user specific information
                string qrData = $"SABI-MARKET-USER-{userId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                using var pngByteQRCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = pngByteQRCode.GetGraphic(20); // 20 pixels per module

                // Convert to base64 for web display
                var qrCodeBase64 = Convert.ToBase64String(qrCodeImage);
                return $"data:image/png;base64,{qrCodeBase64}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for user {UserId}", userId);
                throw;
            }
        }

        // Helper method for generating byte array
     /*   private byte[] GenerateQrCodeImage(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var pngByteQRCode = new PngByteQRCode(qrCodeData);
            return pngByteQRCode.GetGraphic(20);
        }*/

        // Helper methods
        private string GenerateSecureOTP()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                rng.GetBytes(data);
                return Math.Abs(BitConverter.ToInt32(data, 0) % 900000 + 100000).ToString();
            }
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            // Remove non-digit characters and ensure consistent format
            return string.Join("", phoneNumber.Where(char.IsDigit));
        }

        private string MaskIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return "****";

            // For email addresses
            if (identifier.Contains("@"))
            {
                string[] parts = identifier.Split('@');
                if (parts.Length != 2) return "****@**.**"; // Invalid email format

                string username = parts[0];
                string domain = parts[1];

                if (username.Length <= 2)
                    return username + "@" + domain;

                return username.Substring(0, 1) + "****" + username.Substring(username.Length - 1) + "@" + domain;
            }
            // For phone numbers
            else
            {
                if (identifier.Length < 4)
                    return "****";

                return "****" + identifier.Substring(Math.Max(0, identifier.Length - 4));
            }
        }

        private string GetGenericSuccessMessage(DeliveryMethod method)
        {
            return method == DeliveryMethod.SMS
                ? "If your phone number exists in our system, an OTP has been sent"
                : "If your email exists in our system, an OTP has been sent";
        }
    }
}



/* public async Task<BaseResponse<bool>> SendPasswordResetOTPBySMS(string phoneNumber)
       {
           try
           {
               // Find user by phone number
               var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
               if (user == null)
               {
                   // For security reasons, don't reveal that the user doesn't exist
                   return ResponseFactory.Success(true, "If your phone number exists in our system, an OTP has been sent");
               }

               // Check environment (use ENV variable to determine Dev or Production)
               var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
               bool isDevelopment = environment == "Development";

               // Generate OTP: Use static OTP in Dev, Random OTP in Live
               var otp = isDevelopment ? "777777" : new Random().Next(100000, 999999).ToString();

               // Store OTP in user tokens with expiration
               await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "OTP", otp);
               await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "OTPExpiry",
                   DateTime.UtcNow.AddMinutes(10).ToString("o")); // 10-minute expiry

               // Send SMS with OTP
               bool smsSent = await _smsService.SendSMS(
                   phoneNumber,
                   $"Your password reset OTP is: {otp}. Valid for 10 minutes."
               );

               if (!smsSent)
               {
                   _logger.LogWarning($"Failed to send SMS to {phoneNumber}");
                  // return ResponseFactory.Success(true, $"OTP is {otp} and was sent successfully to your phone");
               }

               return ResponseFactory.Success(true, "OTP was sent successfully to your phone");
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error sending password reset OTP via SMS");
               return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
           }
       }
*/

/*public async Task<BaseResponse<bool>> VerifyPasswordResetOTP(string phoneNumber, string otp)
{
    try
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (user == null)
        {
            return ResponseFactory.Fail<bool>(
                new NotFoundException("Invalid phone number or OTP"),
                "Verification failed");
        }

        // Retrieve stored OTP and expiry
        var storedOTP = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "OTP");
        var expiryString = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "OTPExpiry");

        if (string.IsNullOrEmpty(storedOTP) || string.IsNullOrEmpty(expiryString))
        {
            return ResponseFactory.Fail<bool>(
                new BadRequestException("No active OTP found"),
                "Verification failed");
        }

        // Check expiration
        if (DateTime.TryParse(expiryString, out var expiry) && expiry < DateTime.UtcNow)
        {
            return ResponseFactory.Fail<bool>(
                new BadRequestException("OTP has expired"),
                "Verification failed");
        }

        // Verify OTP
        if (storedOTP != otp)
        {
            return ResponseFactory.Fail<bool>(
                new BadRequestException("Invalid OTP"),
                "Verification failed");
        }

        // Generate a reset token
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Store the reset token temporarily
        await _userManager.SetAuthenticationTokenAsync(user, "PasswordReset", "ResetToken", resetToken);

        return ResponseFactory.Success(true, "OTP verified successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error verifying password reset OTP");
        return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
    }
}
*/
/* public async Task<BaseResponse<bool>> ResetPasswordAfterOTP(string phoneNumber, string newPassword)
 {
     try
     {
         var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
         if (user == null)
         {
             return ResponseFactory.Fail<bool>(
                 new NotFoundException("User not found"),
                 "Password reset failed");
         }

         // Retrieve stored reset token
         var resetToken = await _userManager.GetAuthenticationTokenAsync(user, "PasswordReset", "ResetToken");
         if (string.IsNullOrEmpty(resetToken))
         {
             return ResponseFactory.Fail<bool>(
                 new BadRequestException("Invalid or expired verification session"),
                 "Password reset failed");
         }

         // Reset password
         var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
         if (!result.Succeeded)
         {
             return ResponseFactory.Fail<bool>(
                 new BadRequestException(result.Errors.First().Description),
                 "Password reset failed");
         }

         // Clean up tokens
         await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "OTP");
         await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "OTPExpiry");
         await _userManager.RemoveAuthenticationTokenAsync(user, "PasswordReset", "ResetToken");

         return ResponseFactory.Success(true, "Password reset successfully");
     }
     catch (Exception ex)
     {
         _logger.LogError(ex, "Error during password reset");
         return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
     }
 }*/

/*  public async Task<BaseResponse<bool>> ChangePassword(string userId, ChangePasswordDto changePasswordDto)
  {
      try
      {
          var validationResult = await _changePasswordValidator.ValidateAsync(changePasswordDto);
          if (!validationResult.IsValid)
          {
              return ResponseFactory.Fail<bool>(
                  new FluentValidation.ValidationException(validationResult.Errors),
                  "Validation failed");
          }
          var user = await _userManager.FindByIdAsync(userId);
          if (user == null)
          {
              return ResponseFactory.Fail<bool>(
                  new NotFoundException("User not found"),
                  "User not found");
          }
          var result = await _userManager.ChangePasswordAsync(user,
              changePasswordDto.CurrentPassword,
              changePasswordDto.NewPassword);
          if (!result.Succeeded)
          {
              return ResponseFactory.Fail<bool>(
                  new BadRequestException(result.Errors.First().Description),
                  "Password change failed");
          }
          return ResponseFactory.Success(true, "Password changed successfully");
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error during password change");
          return ResponseFactory.Fail<bool>(ex, "An unexpected error occurred");
      }
  }*/