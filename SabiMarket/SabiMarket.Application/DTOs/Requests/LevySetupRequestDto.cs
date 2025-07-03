using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class LevySetupRequestDto
    {
        public string MarketId { get; set; }  // Example: "Ketu Market"

        public MarketTypeEnum MarketType { get; set; } // Example: "Open"

        public MarketTypeEnum TraderOccupancy { get; set; }  // Example: "Shop", "Kiosk", "Open Space"

        public PaymentPeriodEnum PaymentFrequencyDays { get; set; }

        public decimal Amount { get; set; }  // Readonly in Edit Mode
    }

    public class AuditLogDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }

    public class LevySetupResponseDto
    {
        public string Id { get; set; }
        public string TraderName { get; set; }
        public string MarketName { get; set; }  
        public string TraderOccupancy { get; set; }
        public int PaymentFrequencyDays { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
    }

    // Add this DTO to your project
    public class LevyPaymentWithTraderDto
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string TraderName { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }

        // Helper property to get the full name
      /*  public string GetTraderName()
        {
            if (!string.IsNullOrEmpty(TraderFirstName) && !string.IsNullOrEmpty(TraderLastName))
            {
                return $"{TraderFirstName} {TraderLastName}";
            }

            if (!string.IsNullOrEmpty(GoodBoyFirstName) && !string.IsNullOrEmpty(GoodBoyLastName))
            {
                return $"{GoodBoyFirstName} {GoodBoyLastName}";
            }

            return "Unknown";
        }*/
    }
}