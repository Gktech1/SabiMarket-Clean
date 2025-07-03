using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;

[Table("MarketSections")]
public class MarketSection : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public string Description { get; set; }

    public int Capacity { get; set; }

    [Required]
    public string MarketId { get; set; }

    [ForeignKey("MarketId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Market Market { get; set; }

    public virtual ICollection<Trader> Traders { get; set; } = new List<Trader>();
}