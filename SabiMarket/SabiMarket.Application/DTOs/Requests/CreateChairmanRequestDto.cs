namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateChairmanRequestDto
    {
        public string FullName { get; set; }  = string.Empty;   
        public string Email { get; set; }  = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? UserId { get; set; } = string.Empty;
        public string? MarketId { get; set; } = string.Empty;
        public string LocalGovernmentId { get; set; } = string.Empty;
        public string? Title { get; set; } = string.Empty;
        public string? Office { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
    }
}
