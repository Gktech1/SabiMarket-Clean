using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("Markets")]
public class Market : BaseEntity
{
    public string? LocalGovernmentId { get; set; }

    [StringLength(50)]
    public string? MarketType { get; set; }
    public string? ChairmanId { get; set; }

    [StringLength(100)]
    public string? MarketName { get; set; }

    public string? Location { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public string? CaretakerId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalRevenue { get; set; }
    public int PaymentTransactions { get; set; }
    public string? LocalGovernmentName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTraders { get; set; }
    public int MarketCapacity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OccupancyRate { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ComplianceRate { get; set; }
    public int CompliantTraders { get; set; }
    public int NonCompliantTraders { get; set; }
    // Navigation properties
    [ForeignKey("ChairmanId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Chairman Chairman { get; set; }

    [ForeignKey("LocalGovernmentId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual LocalGovernment LocalGovernment { get; set; }

    [ForeignKey("CaretakerId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Caretaker Caretaker { get; set; }

    [NotMapped]
    public virtual ICollection<Caretaker> AdditionalCaretakers { get; set; } = new List<Caretaker>();

    public virtual ICollection<Trader> Traders { get; set; } = new List<Trader>();
    public virtual ICollection<MarketSection> MarketSections { get; set; } = new List<MarketSection>();
}