using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    using System.ComponentModel.DataAnnotations;
    using SabiMarket.Domain.Enum;

    public class UpdateLevySetupRequestDto
    {
        [Required]
        public string LevyId { get; set; } // ID of the levy to update

        public string? MarketId { get; set; }
        public string? MarketType { get; set; }
        public MarketTypeEnum? TraderOccupancy { get; set; }
        public int PaymentFrequencyDays { get; set; } = 0;
        public decimal Amount { get; set; }
    }

    // If you also need a DTO for getting levy details for editing
    public class LevySetupDetailDto
    {
        public string Id { get; set; }
        public string MarketId { get; set; }
        public string MarketName { get; set; }
        public string MarketType { get; set; }
        public MarketTypeEnum TraderOccupancy { get; set; }
        public int PaymentFrequencyDays { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
