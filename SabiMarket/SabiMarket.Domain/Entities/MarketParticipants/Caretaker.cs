using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities.LocalGovernmentAndMArket;
using SabiMarket.Domain.Entities.UserManagement;

namespace SabiMarket.Domain.Entities.MarketParticipants
{

    [Table("Caretakers")]
    public class Caretaker : BaseEntity
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string MarketId { get; set; }

        [Required]
        public string ChairmanId { get; set; }

        public string? LocalGovernmentId { get; set; }   

        public bool IsBlocked { get; set; } = false;

        [ForeignKey("UserId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ApplicationUser User { get; set; }
        [ForeignKey("ChairmanId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Chairman Chairman { get; set; }

        [ForeignKey("LocalGovernmentId")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual LocalGovernment LocalGovernment { get; set; }
        public virtual ICollection<Market> Markets { get; set; } = new List<Market>();
        public virtual ICollection<GoodBoy> GoodBoys { get; set; } = new List<GoodBoy>();
        public virtual ICollection<Trader> AssignedTraders { get; set; } = new List<Trader>();
    }

}