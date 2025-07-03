using SabiMarket.API.Models.Notifications;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Notifications;

namespace SabiMarket.Infrastructure.Services.Notification
{
    public interface IFirebaseNotificationService
    {
        Task<NotificationResponse> SendNotificationAsync(NotificationRequest request);
        Task<NotificationResponse> SendToUserAsync(string userId, NotificationRequest request);
        Task<NotificationResponse> SendToMultipleUsersAsync(List<string> userIds, NotificationRequest request);
        Task<NotificationResponse> SendToTopicAsync(string topic, NotificationRequest request);
        Task<bool> SaveDeviceTokenToDatabase(string userId, string token, string deviceType, string? deviceInfo = null);

        Task<bool> RegisterDeviceTokenAsync(string userId, string token, string deviceType, string? deviceInfo = null);
        Task<bool> RemoveDeviceTokenAsync(string userId, string token);
        Task<bool> CleanupInactiveTokensAsync(int daysThreshold = 30);

        Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId, string userId);

        // Business-specific methods for SabiMarket
        Task<bool> SendOrderNotificationAsync(string userId, string orderId, NotificationType type);
        Task<bool> SendProductNotificationAsync(List<string> userIds, string productId, string message);
        Task<bool> SendPromotionalNotificationAsync(string title, string body, string? imageUrl = null);
    }
}
