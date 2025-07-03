using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SabiMarket.Domain.Entities.WaiveMarketModule;

namespace SabiMarket.Domain.Entities.Supporting
{
    [Table("ProductCategories")]
    public class ProductCategory : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<WaivedProduct> Products { get; set; }
    }
}
