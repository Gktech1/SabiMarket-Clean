using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class SubscriptionPlan : BaseEntity
    {
        public string Frequency { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
