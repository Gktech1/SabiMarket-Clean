using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("SowFoodCompanyStaffAttendances")]
public class SowFoodCompanyStaffAttendance : BaseEntity
{
    [Required]
    public string SowFoodCompanyStaffId { get; set; }

    [Required]
    public DateTime LogonTime { get; set; }

    [Required]
    public DateTime LogoutTime { get; set; }

    public DateTime? ConfirmedTimeIn { get; set; }
    public bool IsConfirmed { get; set; }
    public string? ConfirmedByUserId { get; set; }

    [ForeignKey("ConfirmedByUserId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual ApplicationUser ConfirmedByUser { get; set; }

    [ForeignKey("SowFoodCompanyStaffId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual SowFoodCompanyStaff SowFoodCompanyStaff { get; set; }
}