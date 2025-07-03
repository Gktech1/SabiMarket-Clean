using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class AuditLogFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? User { get; set; }
        public string? Activity { get; set; }
        public string? IpAddress { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public string? SortBy { get; set; }
        public bool? SortDescending { get; set; }
    }
}
