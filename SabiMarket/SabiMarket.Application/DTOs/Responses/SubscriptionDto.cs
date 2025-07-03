using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class SubscriptionDashboadDetailsDto
    {
        public decimal TotalAmount { get; set; }
        public int TotalNumberOfSubscribers { get; set; }
        public int TotalNumberOfConfirmedSubscribers { get; set; }
        public int TotalNumberOfPendingSubscribers { get; set; }

    }
}
