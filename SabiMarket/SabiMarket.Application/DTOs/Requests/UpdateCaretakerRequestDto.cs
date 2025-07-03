using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.MarketParticipants
{
    public class UpdateCaretakerRequestDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Gender { get; set; }

        public string? ProfileImage { get; set; }

        public string? MarketId { get; set; }
    }
}