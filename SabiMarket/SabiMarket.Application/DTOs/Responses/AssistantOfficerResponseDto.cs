using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace SabiMarket.Application.DTOs.Responses
{
    public class AssistantOfficerResponseDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Keep for backward compatibility
        public string MarketId { get; set; }
        public string MarketName { get; set; }

        // New property for multiple markets
        public List<MarketDto> Markets { get; set; } = new List<MarketDto>();

        public string DefaultPassword { get; set; }
        public string Gender { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class MarketDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}