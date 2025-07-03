using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Domain.Entities.WaiveMarketModule
{
    public class WaiveMarketDates : BaseEntity
    {
        public DateTime NextWaiveMarketDate { get; set; }
        public string WaiveMarketLocation { get; set; }
    }
}
