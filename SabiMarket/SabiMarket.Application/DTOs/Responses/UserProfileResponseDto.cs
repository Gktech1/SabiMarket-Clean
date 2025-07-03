namespace SabiMarket.Application.DTOs.Responses
{
    public class UserDetailsResponseDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Gender { get; set; }
        public string Market { get; set; }
        public string LGA { get; set; }
        public string Address { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsBlocked { get; set; }
        public TraderDetailsResponseDto TraderDetails { get; set; }
        public string QrCodeData { get; set; }  // For QR code generation
    }

    // Specific vendor details
    public class VendorDetailsResponseDto : UserDetailsResponseDto
    {
        public string BusinessName { get; set; }
    }

    // Specific trader details
    public class TraderDetailsResponseDto : UserDetailsResponseDto
    {
        public string TraderOccupancy { get; set; }
        public string PaymentFrequency { get; set; }
        public string TraderIdentityNumber { get; set; }
    }
}
