using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("SowFoodCompanyStaffAppraisers")]
public class SowFoodCompanyStaffAppraiser : BaseEntity
{
    [Required]
    public string SowFoodCompanyStaffId { get; set; }

    public string? UserId { get; set; }

    [Required]
    public string Remark { get; set; }

    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual ApplicationUser User { get; set; }

    [ForeignKey("SowFoodCompanyStaffId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual SowFoodCompanyStaff SowFoodCompanyStaff { get; set; }
}