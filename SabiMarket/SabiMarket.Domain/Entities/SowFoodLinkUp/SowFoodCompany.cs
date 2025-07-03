using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

[Table("SowFoodCompanies")]
public class SowFoodCompany : BaseEntity
{
    [Required]
    public string CompanyName { get; set; }
    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual ApplicationUser User { get; set; }

    public virtual ICollection<SowFoodCompanyStaff> Staff { get; set; } = new List<SowFoodCompanyStaff>();
    public virtual ICollection<SowFoodCompanyProductionItem> SowFoodProducts { get; set; } = new List<SowFoodCompanyProductionItem>();
    public virtual ICollection<SowFoodCompanySalesRecord> SowFoodSalesRecords { get; set; } = new List<SowFoodCompanySalesRecord>();
    public virtual ICollection<SowFoodCompanyShelfItem> SowFoodShelfItems { get; set; } = new List<SowFoodCompanyShelfItem>();
    public virtual ICollection<SowFoodCompanyCustomer> Customers { get; set; } = new List<SowFoodCompanyCustomer>();
}