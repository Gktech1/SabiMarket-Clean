using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.PaymentsDto
{
    public class FundWalletVM
    {
        public string UserId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
    public class CreateWithdrawalVM
    {
        public string BankCode { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
    }
    public record Bank(string Name, string Code);

}
