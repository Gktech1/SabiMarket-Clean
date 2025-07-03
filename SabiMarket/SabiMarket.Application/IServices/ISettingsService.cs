using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.IServices
{
    public interface ISettingsService
    {
        Task<BaseResponse<bool>> ChangePassword(string userId, ChangePasswordDto changePasswordDto);
        Task<BaseResponse<bool>> UpdateProfile(string userId, UpdateProfileDto updateProfileDto);
        Task<BaseResponse<UserDetailsResponseDto>> GetUserDetails(string userId, string userType);
        Task<BaseResponse<bool>> SendPasswordResetOTP(ForgotPasswordDto forgotPasswordDto);
        Task<BaseResponse<bool>> VerifyPasswordResetOTP(VerifyOTPDto verifyOTPDto);
        Task<BaseResponse<bool>> ResetPasswordAfterOTP(ResetPasswordDto resetPasswordDto);
        Task<BaseResponse<bool>> LogoutUser(string userId);
    }
}
