using System.ComponentModel.DataAnnotations;
using SabiMarket.Application.DTOs;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Services.Dtos.Levy
{
    // DTOs for Levy Setup
    public class LevySetupResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string MarketId { get; set; }
        public string MarketName { get; set; }
    }

    // DTOs for Dashboard
    public class GoodBoyDashboardStatsDto
    {
        public int TraderCount { get; set; }
        public decimal TotalLevies { get; set; }
        public PaginatorDto<IEnumerable<LevyPaymentDto>> Payments { get; set; }
    }

    public class LevyPaymentDto
    {
        public string PayerName { get; set; }  // This will match "Iya Rokeeb" in the UI
        public DateTime PaymentTime { get; set; }  // This will be formatted as "2:00 pm" in the UI
        public decimal Amount { get; set; }  // This will be formatted as "₦5,000" in the UI
    }

    // DTOs for Levy Payment
    public class LevyPaymentCreateDto
    {
        [Required]
        public string TraderId { get; set; }

        [Required]
        public string GoodBoyId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentPeriodEnum Period { get; set; }

        [Required]
        public PaymenPeriodEnum PaymentMethod { get; set; }

        public bool HasIncentive { get; set; }

        public decimal? IncentiveAmount { get; set; }

        [Required]
        public string Notes { get; set; }

        public string QRCodeScanned { get; set; }
    }

    public class GoodLevyPaymentResponseDto
    {
        public string Id { get; set; }
        public string TraderId { get; set; }
        public string TraderName { get; set; }
        public string TraderBusinessName { get; set; }
        public string GoodBoyId { get; set; }
        public string GoodBoyName { get; set; }
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public decimal Amount { get; set; }
        public PaymentPeriodEnum Period { get; set; }
        public PaymenPeriodEnum PaymentMethod { get; set; }
        public PaymentStatusEnum PaymentStatus { get; set; }
        public string TransactionReference { get; set; }
        public bool HasIncentive { get; set; }
        public decimal? IncentiveAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CollectionDate { get; set; }
        public string Notes { get; set; }
    }
}