using global::SabiMarket.Application.DTOs.Requests;
using global::SabiMarket.Application.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabiMarket.Application.Interfaces;

namespace SabiMarket.API.Controllers.Authentication
{
    namespace SabiMarket.API.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        [Produces("application/json")]
        public class AuthenticationController : ControllerBase
        {
            private readonly IAuthenticationService _authService;
            private readonly ILogger<AuthenticationController> _logger;

            public AuthenticationController(
                IAuthenticationService authService,
                ILogger<AuthenticationController> logger)
            {
                _authService = authService;
                _logger = logger;
            }

            /// <summary>
            /// Authenticates a user and returns a JWT token
            /// </summary>
            /// <param name="request">Login credentials</param>
            /// <returns>Authentication token and user information</returns>
            /// <response code="200">Returns the JWT token and user data</response>
            /// <response code="400">If the credentials are invalid</response>
            /// <response code="401">If the user is unauthorized</response>
            /// <response code="403">If the account is deactivated</response>
            /// <response code="500">If there was an internal server error</response>
            [HttpPost("login")]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status403Forbidden)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
            {
                var response = await _authService.LoginAsync(request);

                if (!response.IsSuccessful)
                {
                    return response.Error?.StatusCode switch
                    {
                        StatusCodes.Status400BadRequest => BadRequest(response),
                        StatusCodes.Status401Unauthorized => Unauthorized(response),
                        StatusCodes.Status403Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
                        StatusCodes.Status404NotFound => NotFound(response),
                        _ => BadRequest(response)
                    };
                }

                return Ok(response);
            }

            /// <summary>
            /// Generate Refresh Token 
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
            [HttpPost("refresh-token")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(BaseResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto request)
            {
                    var response = await _authService.RefreshTokenAsync(request.RefreshToken);
                    if (!response.IsSuccessful)
                    {
                        return BadRequest(response);
                    }
                    return Ok(response);
            }

            /// <summary>
            /// Registers a new user in the system
            /// </summary>
            /// <param name="request">User registration details</param>
            /// <returns>Registration confirmation with user information</returns>
            /// <response code="200">Returns the registration confirmation</response>
            /// <response code="400">If the registration data is invalid</response>
            /// <response code="409">If the email is already registered</response>
            /// <response code="500">If there was an internal server error</response>
            [HttpPost("register")]
            [AllowAnonymous]
            [ProducesResponseType(typeof(BaseResponse<RegistrationResponseDto>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(BaseResponse<RegistrationResponseDto>), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(BaseResponse<RegistrationResponseDto>), StatusCodes.Status409Conflict)]
            [ProducesResponseType(typeof(BaseResponse<RegistrationResponseDto>), StatusCodes.Status500InternalServerError)]
            public async Task<IActionResult> Register([FromBody] RegistrationRequestDto request)
            {
                var response = await _authService.RegisterAsync(request);

                if (!response.IsSuccessful)
                {
                    // Handle different types of registration failures
                    return response.Error?.StatusCode switch
                    {
                        StatusCodes.Status400BadRequest => BadRequest(response),
                        StatusCodes.Status409Conflict => Conflict(response),
                        _ => BadRequest(response)
                    };
                }

                return Ok(response);

            }

            /// <summary>
            /// Test endpoint to verify if authentication is working
            /// </summary>
            /// <returns>A message indicating the authentication status</returns>
            [HttpGet("test")]
            [ProducesResponseType(typeof(BaseResponse<string>), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            public IActionResult TestAuth()
            {
                return Ok(ResponseFactory.Success("Authentication is working!"));
            }
        }
    }
}
