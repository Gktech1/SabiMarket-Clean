using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateSubscriptionDto
    {
        public decimal Amount { get; set; }
        //public string PaymentMethod { get; set; }
        public string ProofOfPayment { get; set; }
        public string SubscriberId { get; set; }
        public string SubscriberType { get; set; }
    }
    public class CreateSubscriptionPlanDto
    {
        public decimal Amount { get; set; }
        public string Frequency { get; set; }

    }
    public class UpdateSubscriptionPlanDto
    {
        public decimal Amount { get; set; }
        public string Frequency { get; set; }
        public string Id { get; set; }

    }
    public class GetSubScriptionDto
    {
        public DateTime SubscriptionDate { get; set; }
        public string Product { get; set; }
        public decimal Amount { get; set; }
    }
}
