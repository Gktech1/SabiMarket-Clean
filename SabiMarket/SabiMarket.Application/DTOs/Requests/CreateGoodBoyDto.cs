using Microsoft.AspNetCore.Http;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateGoodBoyDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public List<string> MarketIds { get; set; } = new List<string>();
        public string ProfileImage { get; set; }
    }

    public class UpdateGoodBoyRequestDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public List<string> MarketIds { get; set; } = new List<string>();
        public string ProfileImage { get; set; }
    }
}
