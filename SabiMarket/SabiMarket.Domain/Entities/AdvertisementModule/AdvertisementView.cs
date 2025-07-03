using System.ComponentModel.DataAnnotations.Schema;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.AdvertisementModule
{
    [Table("AdvertisementViews")]
    public class AdvertisementView : BaseEntity
    {
        public string AdvertisementId { get; set; }
        public string UserId { get; set; }
        public string IPAddress { get; set; }
        public DateTime ViewedAt { get; set; }
        public virtual Advertisement Advertisement { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

}
