using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<BaseResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);
        Task<BaseResponse<RegistrationResponseDto>> RegisterAsync(RegistrationRequestDto request);
        Task<BaseResponse<LoginResponseDto>> RefreshTokenAsync(string refreshToken);
    }
}
