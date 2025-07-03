using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; } = string.Empty;
        public string? EmailAddress { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public string? LocalGovernmentId { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; } = string.Empty; 
    }

}
