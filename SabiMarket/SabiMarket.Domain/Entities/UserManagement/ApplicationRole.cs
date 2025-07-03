namespace SabiMarket.Domain.Entities.UserManagement
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("AspNetRoles")]
    public class ApplicationRole : IdentityRole
    {
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        [MaxLength(100)]
        public string LastModifiedBy { get; set; }

        public virtual ICollection<RolePermission> Permissions { get; set; }

        public ApplicationRole() : base()
        {
            Permissions = new HashSet<RolePermission>();
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            Permissions = new HashSet<RolePermission>();
        }
    }

    [Table("RolePermissions")]
    public class RolePermission
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public bool IsGranted { get; set; }

        [Required]
        public string RoleId { get; set; }

        [ForeignKey("RoleId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationRole Role { get; set; }
    }
}