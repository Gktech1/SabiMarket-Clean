namespace SabiMarket.Application.DTOs.Responses
{
    public class CaretakerResponseDto
    {
        public string Id { get; set; }

        // User Details
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Market Details
        public string MarketId { get; set; }

        public string PhoneNumber { get; set; } 

        public string? Gender { get; set; }  

        public string ProfileImageUrl { get; set; } 

        public string DefaultPassword { get; set; }    
        public MarketResponseDto Market { get; set; }

        // Associated Entities
       // public ICollection<GoodBoyResponseDto> GoodBoys { get; set; }
       // public ICollection<TraderResponseDto> AssignedTraders { get; set; }

        // Audit Fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlocked { get; set; }  
    }
}
