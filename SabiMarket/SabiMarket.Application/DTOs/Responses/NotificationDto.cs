using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class NotificationDto
    {
        public string Id { get; set; }
        public string VendorId { get; set; }
        public string CustomerId { get; set; }
        public string Message { get; set; }
        public string? VendorResponse { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
