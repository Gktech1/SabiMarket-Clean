using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class LocalGovernmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StateCode { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int TotalMarkets { get; set; }
        public int ActiveMarkets { get; set; }
        public decimal TotalRevenue { get; set; }
        public ChairmanDto Chairman { get; set; }
    }

    public class ChairmanDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string LastLoginAt { get; set; }
    }

}
