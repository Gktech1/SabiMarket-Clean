using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class LGAResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StateId { get; set; }
        public string StateName { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Statistics
        public int TotalMarkets { get; set; }
        public int ActiveMarkets { get; set; }
        public int TotalTraders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
