namespace SabiMarket.Application.DTOs.Responses
{
    public class QRCodeResponseDto
    {
        public string QRCodeImage { get; set; }  // Base64 encoded PNG image
        public string QRCodeData { get; set; }   // The data encoded in the QR code
        public string TraderId { get; set; }
        public string TraderName { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
