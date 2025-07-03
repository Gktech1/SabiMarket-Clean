using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CaretakerForCreationRequestDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string MarketId { get; set; }
        public string Gender { get; set; }
        public string? PhotoUrl { get; set; }
        public string? LocalGovernmentId { get; set; } 
    }
}
