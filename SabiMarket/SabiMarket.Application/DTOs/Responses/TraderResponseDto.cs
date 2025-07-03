using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Responses
{
    public class TraderResponseDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public string Gender { get; set; }
        public string IdentityNumber { get; set; }
        public string BusinessName { get; set; }
        public string DefaultPassword { get; set; }
        public string ProfileImageUrl { get; set; }
        public string TraderName { get; set; }
        public string BusinessType { get; set; }
        public List<BuildingTypeEnum> BuildingTypes { get; set; }
        public DateTime DateAdded { get; set; }
        public string QRCode { get; set; }
        public bool IsActive { get; set; }
    }
    /*  public class TraderResponseDto
      {
          public string Id { get; set; }
          public string FullName { get; set; }
          public string PhoneNumber { get; set; }
          public string Email { get; set; }
          public string MarketId { get; set; }
          public string MarketName { get; set; }
          public string Gender { get; set; }
          public string IdentityNumber { get; set; }
          public string BusinessName {  get; set; }
          public string DefaultPassword { get; set; }
          public string ProfileImageUrl { get; set; } 
          public string TraderName { get; set; }    
          public string BusinessType { get; set; }  
          public int BuildingTypes {  get; set; }
          public DateTime DateAdded { get; set; }
          public string QRCode { get; set; }
          public bool IsActive { get; set; }
      }*/

    public class TraderDetailsDto : TraderResponseDto
    {
        public string? Address { get; set; }
        public decimal? TotalLeviesPaid { get; set; }
        public DateTime? LastLevyPayment { get; set; }
        public string? PaymentStatus { get; set; }
        public ICollection<LevyResponseDto> RecentPayments { get; set; }

        // For profile screen (first UI)
        public decimal? CurrentLevyAmount { get; set; } // The ₦500 amount
        public string? PaymentFrequencyDisplay { get; set; } // "2 days - ₦500"
        public bool PushNotificationsEnabled { get; set; } // Toggle state

        // For details screen (second UI)
        public string? TraderPhoneNumber { get; set; } // Separate from PhoneNumber if needed
        public string? DateAddedFormatted { get; set; } // "Oct 29, 2024, 10:00 am"
        public string? TraderIdentityNumber { get; set; } // "OSHILAG/23401"

        // Common QR Code fields
        public bool HasQRCode { get; set; }
        public string? QRCodeImageUrl { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    // Helper DTO for payment frequency calculation
    public class PaymentFrequencyInfo
    {
        public decimal Amount { get; set; }
        public int Days { get; set; }
        public string PaymentFrequency => $"{Days} days - ₦{Amount:N0}";
    }
/*
    public class TraderDetailsDto : TraderResponseDto
    {
        public string Address { get; set; }
        public string BusinessType { get; set; }
        public decimal TotalLeviesPaid { get; set; }
        public DateTime LastLevyPayment { get; set; }
        public string PaymentStatus { get; set; }
        public ICollection<LevyResponseDto> RecentPayments { get; set; }
    }*/
}

