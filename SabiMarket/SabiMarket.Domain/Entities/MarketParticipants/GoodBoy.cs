using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.LevyManagement;
using SabiMarket.Domain.Entities.MarketParticipants;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

[Table("GoodBoys")]
public class GoodBoy : BaseEntity
{
    [Required]
    public string UserId { get; set; }

    public string? CaretakerId { get; set; }

    [Required]
    public string MarketId { get; set; }

    [Required]
    public StatusEnum Status { get; set; }

    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual ApplicationUser User { get; set; }

    [ForeignKey("MarketId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Market Market { get; set; }

    [ForeignKey("CaretakerId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Caretaker Caretaker { get; set; }

    public virtual ICollection<LevyPayment> LevyPayments { get; set; } = new List<LevyPayment>();
}