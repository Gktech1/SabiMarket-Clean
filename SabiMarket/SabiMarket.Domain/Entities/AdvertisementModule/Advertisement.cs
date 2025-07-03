using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.AdvertisementModule;
using SabiMarket.Domain.Entities.WaiveMarketModule;
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Advertisements")]
public class Advertisement : BaseEntity
{
    [Required]
    public string VendorId { get; set; }

    [Required]
    public string AdminId { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    public string ImageUrl { get; set; }
    public string TargetUrl { get; set; }
    public AdvertStatusEnum Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public string Language { get; set; }

    [Required]
    public string Location { get; set; }

    [Required]
    public string AdvertPlacement { get; set; }

    public string PaymentStatus { get; set; }
    public string PaymentProofUrl { get; set; }
    public string BankTransferReference { get; set; }

    [ForeignKey("VendorId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Vendor Vendor { get; set; }

    [ForeignKey("AdminId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Admin Admin { get; set; }

    public virtual ICollection<AdvertisementView> Views { get; set; } = new List<AdvertisementView>();
    public virtual ICollection<AdvertisementLanguage> Translations { get; set; } = new List<AdvertisementLanguage>();
    public virtual AdvertPayment Payment { get; set; }
}