using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateGoodBoyRequestDto
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string CaretakerId { get; set; }
        public string MarketId { get; set; }
        public string? ProfileImage { get; set; }
        public string? Gender { get; set; } 
        public string? LocalGovernmentId { get; set; }  
    }

    public class UpdateGoodBoyProfileDto
    {
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }

        public string? ProfileImage { get; set; }

        public bool? IsAvailableForDuty { get; set; }

    }

    public class GoodBoyFilterRequestDto
    {
        public string MarketId { get; set; }
        public string CaretakerId { get; set; }
        public StatusEnum? Status { get; set; }
    }

    public class ProcessLevyPaymentDto
    {
        public string GoodBoyId { get; set; }
        public decimal Amount { get; set; }
        public PaymentPeriodEnum Period { get; set; }
        public PaymenPeriodEnum? PaymentMethod { get; set; }
        public bool HasIncentive { get; set; }
        public decimal? IncentiveAmount { get; set; }
        public string? TransactionReference { get; set; }
        public MarketTypeEnum OccupancyType { get; set; }
        public string? Notes { get; set; }
        public string QRCodeScanned { get; set; }
    }

    public class ProcessAsstOfficerLevyPaymentDto
    {
        public string AssistOfficerId { get; set; }
        public decimal Amount { get; set; }
        public PaymentPeriodEnum Period { get; set; }
        public PaymenPeriodEnum? PaymentMethod { get; set; }
        public MarketTypeEnum OccupancyType { get; set; }
        public bool HasIncentive { get; set; }
        public decimal? IncentiveAmount { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
        public string QRCodeScanned { get; set; }
    }
}
