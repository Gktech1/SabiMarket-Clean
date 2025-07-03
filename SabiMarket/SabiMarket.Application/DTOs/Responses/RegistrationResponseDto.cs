namespace SabiMarket.Application.DTOs.Responses
{
    public class RegistrationResponseDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Message { get; set; }
        public bool RequiresEmailVerification { get; set; }
        public bool RequiresApproval { get; set; }
    }
}
