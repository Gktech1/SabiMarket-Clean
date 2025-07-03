using SabiMarket.Domain.Enum;

namespace SabiMarket.Application.DTOs.Requests
{
    public class PromotionalNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class OrderNotificationRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
    }
}
