using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.AdvertisementModule
{

    [Table("AdvertPayments")]
    public class AdvertPayment : BaseEntity
    {
        public string AdvertisementId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } // Bank Transfer

        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        public string Status { get; set; }
        public string ProofOfPaymentUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }

        public virtual Advertisement Advertisement { get; set; }
    }
}
