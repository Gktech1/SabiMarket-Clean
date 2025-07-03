using SabiMarket.Domain.Enum;

namespace SabiMarket.API.Models.Notifications
{
    public class NotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Token { get; set; }
        public List<string>? Tokens { get; set; }
        public string? Topic { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? MessageId { get; set; }
        public List<string>? FailedTokens { get; set; }
    }

   // Request DTOs for API endpoints
    public class RegisterTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
    }

    public class RemoveTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class SendNotificationToUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public NotificationType Type { get; set; } = NotificationType.General;
        public string? ActionUrl { get; set; }
    }

    public class SendBulkNotificationRequest
    {
        public List<string> UserIds { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public Dictionary<string, string>? Data { get; set; }
        public NotificationType Type { get; set; } = NotificationType.General;
    }
}