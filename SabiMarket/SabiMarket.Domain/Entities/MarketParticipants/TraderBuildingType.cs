using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Domain.Entities.MarketParticipants
{
    [Table("TraderBuildingTypes")]
    public class TraderBuildingType : BaseEntity
    {
        [Required]
        public string TraderId { get; set; }

        [Required]
        public BuildingTypeEnum BuildingType { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfBuildingTypes { get; set; }

        // Navigation property
        [ForeignKey("TraderId")]
        [DeleteBehavior(DeleteBehavior.Cascade)]
        public virtual Trader Trader { get; set; }
    }
}
