namespace SabiMarket.Application.DTOs.Requests
{
    public class ScanTraderQRCodeDto
    {
        public string? QRCodeData { get; set; } = string.Empty;
        public string ScannedByUserId { get; set; } = string.Empty;
        public string? MarketId { get; set; }   = string.Empty;
        public string? TraderId { get; set; }
    }
}
