using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class UpdateTraderMarketDto
    {
        public string? TraderName { get; set; }
        public string? TraderOccupancy { get; set; }
        public string? TraderIdentityNumber { get; set; }
        public string? PaymentFrequency { get; set; }
        public string? LastPaymentDate { get; set; }
        public string? UpdatePaymentUrl { get; set; }
        [Required]
        public string MarketId { get; set; }
        public string? PhoneNumber { get; set; }

        public int NumberOfBuildingTypes { get; set; }  = 0;    

        public string? EmailAddress { get; set; }
        public string? Address { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
