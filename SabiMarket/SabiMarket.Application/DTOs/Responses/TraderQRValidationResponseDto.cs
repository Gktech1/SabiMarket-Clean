using SabiMarket.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class TraderQRValidationResponseDto
    {
        public string TraderId { get; set; }
        public string TraderName { get; set; }
        public string TraderOccupancy { get; set; }
        public string TraderIdentityNumber { get; set; }
        public string PaymentFrequency { get; set; }
        public string? MarketId { get; set; }
        public string? MarketName { get; set; }
        public PaymentPeriodEnum PaymentPeriod { get; set; } = PaymentPeriodEnum.Weekly;
        public decimal TotalAmount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string? UpdatePaymentUrl { get; set; }

        public string? ProfileImageUrl { get; set; }
        // Properties to match UI
        public int NumberOfBuildingTypes { get; set; }
        public LevyBreakdownDto LevyBreakdown { get; set; }

        // Additional properties that might be useful
        public string BusinessName { get; set; }
        public MarketTypeEnum OccupancyType { get; set; }
    }

    public class LevyBreakdownDto
    {
        public decimal CurrentOpenSpaceLevy { get; set; }
        public decimal TotalUnpaidOpenSpaceLevy { get; set; } = 1000; // Default from UI
        public decimal CurrentKioskLevy { get; set; }
        public decimal TotalUnpaidKioskLevy { get; set; }
        public decimal CurrentShopLevy { get; set; } // Added for Shop type
        public decimal TotalUnpaidShopLevy { get; set; }

        public decimal CurrentWareHouseLevy { get; set; } // Added for Shop type
        public decimal TotalUnpaidwareHouseLevy { get; set; }
        public decimal TotalAmount { get; set; }

        // Additional breakdown details
        public string PaymentStatus { get; set; }
        public int OverdueDays { get; set; }
    }
    /* public class TraderQRValidationResponseDto
     {
         public string TraderId { get; set; }
         public string TraderName { get; set; }
         public string TraderOccupancy { get; set; }
         public string TraderIdentityNumber { get; set; }
         public string PaymentFrequency { get; set; }
         public string? MarketId { get; set; }
         public string? MarketName { get; set; }
         public PaymentPeriodEnum PayementPeriod { get; set; } = PaymentPeriodEnum.Weekly;  
         public decimal Amount { get; set; }
         public DateTime? LastPaymentDate { get; set; }
         public string? UpdatePaymentUrl { get; set; }    
     }*/
}
