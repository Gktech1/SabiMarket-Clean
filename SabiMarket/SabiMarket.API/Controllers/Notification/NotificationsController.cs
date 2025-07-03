using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SabiMarket.API.Models.Notifications;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Services.Notification;
using Mailjet.Client.Resources;
using FirebaseAdmin;
using SabiMarket.Application.DTOs.Requests;

namespace SabiMarket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IFirebaseNotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            IFirebaseNotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }


        // Controller usage - auto-detect platform
        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterTokenRequest request)
        {
            var userId = GetCurrentUserId();
            var userAgent = Request.Headers.UserAgent.ToString();

            // This will auto-detect if it's web, Android, or iOS
            var result = await _notificationService.RegisterDeviceTokenAsync(userId, request.Token, userAgent);

            return result ? Ok() : BadRequest();
        }

      /*  /// <summary>
        /// Register a device token for push notifications
        /// </summary>
        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                var result = await _notificationService.RegisterDeviceTokenAsync(
                    userId, request.Token, request.DeviceType, request.DeviceInfo);

                return result ? Ok(new { success = true, message = "Device token registered successfully" })
                             : BadRequest(new { success = false, message = "Failed to register device token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }*/

        /// <summary>
        /// Remove a device token
        /// </summary>
        [HttpPost("remove-token")]
        public async Task<IActionResult> RemoveDeviceToken([FromBody] RemoveTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                var result = await _notificationService.RemoveDeviceTokenAsync(userId, request.Token);

                return result ? Ok(new { success = true, message = "Device token removed successfully" })
                             : BadRequest(new { success = false, message = "Failed to remove device token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing device token for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user notifications with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
                var unreadCount = await _notificationService.GetUnreadNotificationCountAsync(userId);

                var response = notifications.Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    body = n.Body,
                    imageUrl = n.ImageUrl,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt,
                    type = n.Type.ToString(),
                    actionUrl = n.ActionUrl,
                    data = !string.IsNullOrEmpty(n.DataJson)
                        ? Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(n.DataJson)
                        : null
                }).ToList();

                return Ok(new
                {
                    notifications = response,
                    page,
                    pageSize,
                    totalCount = notifications.Count,
                    unreadCount,
                    hasMore = notifications.Count == pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Mark a specific notification as read
        /// </summary>
        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkNotificationAsReadAsync(id, userId);

                return result ? Ok(new { success = true, message = "Notification marked as read" })
                             : NotFound(new { success = false, message = "Notification not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", id, GetCurrentUserId());
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Mark all notifications as read for the current user
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);

                return result ? Ok(new { success = true, message = "All notifications marked as read" })
                             : BadRequest(new { success = false, message = "Failed to mark notifications as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get unread notification count for the current user
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);

                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching unread count for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.DeleteNotificationAsync(id, userId);

                return result ? Ok(new { success = true, message = "Notification deleted successfully" })
                             : NotFound(new { success = false, message = "Notification not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", id, GetCurrentUserId());
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // Admin endpoints for sending notifications
        /// <summary>
        /// Send notification to a specific user (Admin only)
        /// </summary>
        [HttpPost("send-to-user")]
        [Authorize(Roles = "Admin,Chairman")]
        public async Task<IActionResult> SendToUser([FromBody] SendNotificationToUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notificationRequest = new NotificationRequest
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl,
                    Data = request.Data
                };

                var result = await _notificationService.SendToUserAsync(request.UserId, notificationRequest);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", request?.UserId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Send notification to multiple users (Admin only)
        /// </summary>
        [HttpPost("send-bulk")]
        [Authorize(Roles = "Admin,Chairman")]
        public async Task<IActionResult> SendBulkNotification([FromBody] SendBulkNotificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var notificationRequest = new NotificationRequest
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl,
                    Data = request.Data
                };

                var result = await _notificationService.SendToMultipleUsersAsync(request.UserIds, notificationRequest);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Send promotional notification to all users (Admin only)
        /// </summary>
        [HttpPost("send-promotional")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendPromotionalNotification([FromBody] PromotionalNotificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _notificationService.SendPromotionalNotificationAsync(
                    request.Title, request.Body, request.ImageUrl);

                return result ? Ok(new { success = true, message = "Promotional notification sent successfully" })
                             : BadRequest(new { success = false, message = "Failed to send promotional notification" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending promotional notification");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Send order notification (Internal use - called by order service)
        /// </summary>
        [HttpPost("send-order-notification")]
        [Authorize(Roles = "Admin,Trader,Vendor")]
        public async Task<IActionResult> SendOrderNotification([FromBody] OrderNotificationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _notificationService.SendOrderNotificationAsync(
                    request.UserId, request.OrderId, request.Type);

                return result ? Ok(new { success = true, message = "Order notification sent successfully" })
                             : BadRequest(new { success = false, message = "Failed to send order notification" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order notification for order {OrderId}", request?.OrderId);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cleanup inactive device tokens (Admin only)
        /// </summary>
        [HttpPost("cleanup-tokens")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupInactiveTokens([FromQuery] int daysThreshold = 30)
        {
            try
            {
                var result = await _notificationService.CleanupInactiveTokensAsync(daysThreshold);

                return result ? Ok(new { success = true, message = "Token cleanup completed successfully" })
                             : BadRequest(new { success = false, message = "Failed to cleanup tokens" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up inactive tokens");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get notification statistics (Admin only)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetNotificationStats()
        {
            try
            {
                // You can implement additional stats logic here
                return Ok(new
                {
                    success = true,
                    message = "Stats endpoint - implement additional logic as needed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notification stats");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet("test-config")]
        [AllowAnonymous]
        public IActionResult TestFirebaseConfig()
        {
            try
            {
                var app = FirebaseApp.DefaultInstance;
                return Ok(new
                {
                    success = true,
                    message = "Firebase working!",
                    projectId = "sabimarket-12443"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   User.FindFirst("sub")?.Value ??
                   User.FindFirst("userId")?.Value ??
                   User.FindFirst("id")?.Value ??
                   throw new UnauthorizedAccessException("User ID not found in claims");
        }
    }
  
}