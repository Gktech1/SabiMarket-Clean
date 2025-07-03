using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateMarketRequestDto
    {
        public string MarketName { get; set; }
        public MarketTypeEnum MarketType { get; set; }
        public string? CaretakerId { get; set; }
    }

    public class UpdateMarketRequestDto
    {
        public string? MarketName { get; set; }
        public MarketTypeEnum? MarketType { get; set; }
        public string? CaretakerId { get; set; }
    }



    public class MarketDetailsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int TotalTraders { get; set; }
        public int Capacity { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<CaretakerResponseDto> Caretakers { get; set; }
        public ICollection<TraderResponseDto> Traders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ComplianceRate { get; set; }
    }
}