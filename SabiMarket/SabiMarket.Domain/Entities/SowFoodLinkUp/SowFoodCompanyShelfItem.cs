using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("SowFoodCompanyShelfItems")]
public class SowFoodCompanyShelfItem : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public string ImageUrl { get; set; }

    [Required]
    public string SowFoodCompanyId { get; set; }

    // Navigation property
    [ForeignKey("SowFoodCompanyId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual SowFoodCompany SowFoodCompany { get; set; }
}