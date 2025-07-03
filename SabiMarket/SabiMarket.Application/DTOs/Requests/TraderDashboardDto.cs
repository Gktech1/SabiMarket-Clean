using SabiMarket.Application.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class TraderDashboardResponseDto
    {
        public string TraderName { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public decimal TotalLeviesPaid { get; set; }
        public List<TraderLevyPaymentDto> RecentLevyPayments { get; set; } = new List<TraderLevyPaymentDto>();
    }

    public class TraderLevyPaymentDto
    {
        public string Id { get; set; }
        public string Type { get; set; } // "2 days Levy", "1 week Levy", "Monthly Levy", etc.
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

    public class TraderDashboardRequestDto
    {
        [Required]
        public string TraderId { get; set; }
    }

    public class TraderLevyPaymentsRequestDto
    {
        [Required]
        public string TraderId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchQuery { get; set; }
        public PaginationFilter Pagination { get; set; } = new PaginationFilter();
    }
