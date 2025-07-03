using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("SowFoodCompanyProductionItems")]
public class SowFoodCompanyProductionItem : BaseEntity
{
    [Required]
    public string Name { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public string ImageUrl { get; set; }

    [Required]
    public string SowFoodCompanyId { get; set; }

    [ForeignKey("SowFoodCompanyId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual SowFoodCompany SowFoodCompany { get; set; }
}

