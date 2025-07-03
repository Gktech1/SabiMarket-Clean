using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class Transaction : BaseEntity
    {
        //public string WalletId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public string SenderId { get; set; } = string.Empty;
        //public string ReceiverId { get; set; } = string.Empty;
        public string TransactionType { get; set; } = TransactionTypes.Funding.ToString();
        public string Description { get; set; } = string.Empty;

        public bool Status { get; set; }
        public string Reference { get; set; }

        // Navigation Props
        public virtual ApplicationUser? Sender { get; set; }
    }
}
