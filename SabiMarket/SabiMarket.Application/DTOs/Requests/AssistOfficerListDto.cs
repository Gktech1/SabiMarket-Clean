using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Domain.DTOs
{
    public class AssistOfficerListDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public string LocalGovernmentId { get; set; }
        public string LocalGovernmentName { get; set; }
        public string? ProfileImageUrl { get; set; } 
        public string UserLevel { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AssistOfficerDetailsDto : AssistOfficerListDto
    {
        public string ChairmanId { get; set; }
        public string ChairmanName { get; set; }
        public string Gender { get; set; }
    }

    public class CreateAssistOfficerDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string ChairmanId { get; set; }

        public string MarketId { get; set; }

        public string UserLevel { get; set; }

        [Required]
        public string LocalGovernmentId { get; set; }
    }

    public class UpdateAssistOfficerDto
    {
        public string ChairmanId { get; set; }
        public string MarketId { get; set; }
        public string LocalGovernmentId { get; set; }
        public string UserLevel { get; set; }
        public bool? IsBlocked { get; set; }
    }
}