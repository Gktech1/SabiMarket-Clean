namespace SabiMarket.Application.DTOs.Responses
{
    public class ReportResponseDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MarketId { get; set; }
        public string ChairmanId { get; set; }
    }
}
