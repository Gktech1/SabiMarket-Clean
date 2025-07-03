using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SabiMarket.API.Models.Notifications;
using SabiMarket.Domain.Enum;
using SabiMarket.Domain.Notifications;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Services.Notification;

namespace SabiMarket.API.Services.Notifications
{
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FirebaseNotificationService> _logger;
        private readonly FirebaseApp _firebaseApp;

        public FirebaseNotificationService(
    ApplicationDbContext context,
    ILogger<FirebaseNotificationService> logger,
    IConfiguration configuration)
        {
            _context = context;
            _logger = logger;

            if (FirebaseApp.DefaultInstance == null)
            {
                GoogleCredential credential;

                // Try environment variable first (production)
                var serviceAccountJson = configuration["Firebase:ServiceAccountJson"];
                if (!string.IsNullOrEmpty(serviceAccountJson))
                {
                    credential = GoogleCredential.FromJson(serviceAccountJson);
                }
                else
                {
                    // Fallback to file path (development)
                    var serviceAccountKeyPath = configuration["Firebase:ServiceAccountKeyPath"];
                    if (File.Exists(serviceAccountKeyPath))
                    {
                        credential = GoogleCredential.FromFile(serviceAccountKeyPath);
                    }
                    else
                    {
                        throw new FileNotFoundException($"Firebase service account not found: {serviceAccountKeyPath}");
                    }
                }

                _firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = credential,
                    ProjectId = configuration["Firebase:ProjectId"]
                });
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }
        /*  public FirebaseNotificationService(
              ApplicationDbContext context,
              ILogger<FirebaseNotificationService> logger,
              IConfiguration configuration)
          {
              _context = context;
              _logger = logger;

              if (FirebaseApp.DefaultInstance == null)
              {
                  var serviceAccountKeyPath = configuration["Firebase:ServiceAccountKeyPath"];
                  if (File.Exists(serviceAccountKeyPath))
                  {
                      var credential = GoogleCredential.FromFile(serviceAccountKeyPath);

                      _firebaseApp = FirebaseApp.Create(new AppOptions()
                      {
                          Credential = credential,
                          ProjectId = configuration["Firebase:ProjectId"]
                      });
                  }
                  else
                  {
                      _logger.LogWarning("Firebase service account key file not found at: {Path}", serviceAccountKeyPath);
                      throw new FileNotFoundException($"Firebase service account key not found: {serviceAccountKeyPath}");
                  }
              }
              else
              {
                  _firebaseApp = FirebaseApp.DefaultInstance;
              }
          }*/

        // Update your BuildMessage method to include enhanced web push support
        /*     private Message BuildMessage(NotificationRequest request)
             {
                 var message = new Message()
                 {
                     Notification = new Notification()
                     {
                         Title = request.Title,
                         Body = request.Body,
                         ImageUrl = request.ImageUrl
                     },
                     Android = new AndroidConfig()
                     {
                         Notification = new AndroidNotification()
                         {
                             Icon = "ic_notification",
                             Color = "#1DA1F2",
                             Sound = "default",
                             ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                             Priority = Priority.High
                         },
                         Priority = Priority.High
                     },
                     Apns = new ApnsConfig()
                     {
                         Aps = new Aps()
                         {
                             Alert = new ApsAlert()
                             {
                                 Title = request.Title,
                                 Body = request.Body
                             },
                             Sound = "default",
                             Badge = 1,
                             Category = "GENERAL"
                         },
                         Headers = new Dictionary<string, string>
                         {
                             ["apns-priority"] = "10"
                         }
                     },
                     Webpush = new WebpushConfig()
                     {
                         Headers = new Dictionary<string, string>
                         {
                             ["TTL"] = "86400", // 24 hours
                             ["Urgency"] = "high"
                         },
                         Notification = new WebpushNotification()
                         {
                             Title = request.Title,
                             Body = request.Body,
                             Icon = request.ImageUrl ?? "/icons/notification-icon.png",
                             Badge = "/icons/badge-icon.png",
                             Image = request.ImageUrl,
                             RequireInteraction = true,
                             Silent = false,
                             Tag = "sabimarket-notification",
                             Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                             Actions = new[]
                             {
                         new WebpushNotificationAction()
                         {
                             Action = "view",
                             Title = "View",
                             Icon = "/icons/view-icon.png"
                         },
                         new WebpushNotificationAction()
                         {
                             Action = "dismiss",
                             Title = "Dismiss",
                             Icon = "/icons/dismiss-icon.png"
                         }
                     }
                         },
                         FcmOptions = new WebpushFcmOptions()
                         {
                             Link = GetActionUrl(request.Data)
                         }
                     }
                 };

                 // Add custom data payload
                 if (request.Data?.Any() == true)
                 {
                     message.Data = new Dictionary<string, string>(request.Data);

                     // Add additional metadata for web clients
                     message.Data["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                     message.Data["platform"] = "web";
                     message.Data["version"] = "1.0";
                 }
                 else
                 {
                     message.Data = new Dictionary<string, string>
                     {
                         ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                         ["platform"] = "web",
                         ["version"] = "1.0"
                     };
                 }

                 return message;
             }
     */


        // Helper method to get action URL from data
        private string GetActionUrl(Dictionary<string, string>? data)
        {
            if (data?.ContainsKey("actionUrl") == true)
            {
                return data["actionUrl"];
            }

            // Default redirect URL for your SabiMarket app
            return "/notifications";
        }

      
        // Update your existing SendNotificationAsync method to include web-specific handling
        public async Task<NotificationResponse> SendNotificationAsync(NotificationRequest request)
        {
            try
            {
                var message = BuildMessage(request);
                var messaging = FirebaseMessaging.GetMessaging(_firebaseApp);

                if (!string.IsNullOrEmpty(request.Token))
                {
                    message.Token = request.Token;
                    var response = await messaging.SendAsync(message);

                    return new NotificationResponse
                    {
                        Success = true,
                        Message = "Notification sent successfully",
                        MessageId = response
                    };
                }
                else if (request.Tokens?.Any() == true)
                {
                    // Split tokens by platform for optimized delivery
                    var webTokens = await GetTokensByDeviceType(request.Tokens, "Web");
                    var mobileTokens = request.Tokens.Except(webTokens).ToList();

                    var results = new List<NotificationResponse>();

                    // Send to web browsers with web-optimized config
                    if (webTokens.Any())
                    {
                        var webMessage = BuildWebOptimizedMessage(request);
                        var webMulticast = new MulticastMessage()
                        {
                            Notification = webMessage.Notification,
                            Data = webMessage.Data,
                            Tokens = webTokens,
                            Webpush = webMessage.Webpush
                        };

                        var webResponse = await messaging.SendMulticastAsync(webMulticast);
                        await CleanupInvalidTokens(webTokens, webResponse);

                        results.Add(new NotificationResponse
                        {
                            Success = webResponse.SuccessCount > 0,
                            Message = $"Web: {webResponse.SuccessCount}/{webTokens.Count}",
                            FailedTokens = GetFailedTokens(webTokens, webResponse)
                        });
                    }

                    // Send to mobile devices with mobile config
                    if (mobileTokens.Any())
                    {
                        var mobileMulticast = new MulticastMessage()
                        {
                            Notification = message.Notification,
                            Data = message.Data,
                            Tokens = mobileTokens,
                            Android = message.Android,
                            Apns = message.Apns
                        };

                        var mobileResponse = await messaging.SendMulticastAsync(mobileMulticast);
                        await CleanupInvalidTokens(mobileTokens, mobileResponse);

                        results.Add(new NotificationResponse
                        {
                            Success = mobileResponse.SuccessCount > 0,
                            Message = $"Mobile: {mobileResponse.SuccessCount}/{mobileTokens.Count}",
                            FailedTokens = GetFailedTokens(mobileTokens, mobileResponse)
                        });
                    }

                    // Combine results
                    var totalSuccess = results.Sum(r => r.Success ? 1 : 0);
                    var allFailedTokens = results.SelectMany(r => r.FailedTokens ?? new List<string>()).ToList();

                    return new NotificationResponse
                    {
                        Success = totalSuccess > 0,
                        Message = $"Sent to {results.Count} platform(s): {string.Join(", ", results.Select(r => r.Message))}",
                        FailedTokens = allFailedTokens
                    };
                }
                else if (!string.IsNullOrEmpty(request.Topic))
                {
                    message.Topic = request.Topic;
                    var response = await messaging.SendAsync(message);

                    return new NotificationResponse
                    {
                        Success = true,
                        Message = "Topic notification sent successfully",
                        MessageId = response
                    };
                }

                return new NotificationResponse
                {
                    Success = false,
                    Message = "No valid target specified (token, tokens, or topic)"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Firebase notification");
                return new NotificationResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        // Fixed BuildMessage method - compatible with Firebase Admin SDK
        private Message BuildMessage(NotificationRequest request)
        {
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl
                },
                Android = new AndroidConfig()
                {
                    Notification = new AndroidNotification()
                    {
                        Icon = "ic_notification",
                        Color = "#1DA1F2",
                        Sound = "default",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    },
                    Priority = Priority.High
                },
                Apns = new ApnsConfig()
                {
                    Aps = new Aps()
                    {
                        Alert = new ApsAlert()
                        {
                            Title = request.Title,
                            Body = request.Body
                        },
                        Sound = "default",
                        Badge = 1
                    },
                    Headers = new Dictionary<string, string>
                    {
                        ["apns-priority"] = "10"
                    }
                },
                Webpush = new WebpushConfig()
                {
                    Headers = new Dictionary<string, string>
                    {
                        ["TTL"] = "86400", // 24 hours
                        ["Urgency"] = "high"
                    },
                    Notification = new WebpushNotification()
                    {
                        Title = request.Title,
                        Body = request.Body,
                        Icon = request.ImageUrl ?? "/icons/notification-icon.png",
                        Badge = "/icons/badge-icon.png",
                        Image = request.ImageUrl,
                        RequireInteraction = true,
                        Silent = false,
                        Tag = "sabimarket-notification"
                        // Removed Actions - not supported in this SDK version
                    },
                    FcmOptions = new WebpushFcmOptions()
                    {
                        Link = GetActionUrl(request.Data)
                    }
                }
            };

            // Create a new dictionary for Data (not modify existing read-only)
            var messageData = new Dictionary<string, string>();

            if (request.Data?.Any() == true)
            {
                foreach (var kvp in request.Data)
                {
                    messageData[kvp.Key] = kvp.Value;
                }
            }

            // Add additional metadata for web clients
            messageData["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            messageData["platform"] = "web";
            messageData["version"] = "1.0";

            message.Data = messageData;

            return message;
        }

        // Fixed BuildWebOptimizedMessage method
        private Message BuildWebOptimizedMessage(NotificationRequest request)
        {
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl
                },
                Webpush = new WebpushConfig()
                {
                    Headers = new Dictionary<string, string>
                    {
                        ["TTL"] = "86400",
                        ["Urgency"] = "high"
                    },
                    Notification = new WebpushNotification()
                    {
                        Title = request.Title,
                        Body = request.Body,
                        Icon = request.ImageUrl ?? "/icons/notification-icon.png",
                        Badge = "/icons/badge-icon.png",
                        Image = request.ImageUrl,
                        RequireInteraction = true,
                        Silent = false,
                        Tag = "sabimarket-notification"
                        // Removed Actions - not supported in this SDK version
                    },
                    FcmOptions = new WebpushFcmOptions()
                    {
                        Link = GetActionUrl(request.Data)
                    }
                }
            };

            // Create new dictionary for enhanced data
            var messageData = new Dictionary<string, string>();

            if (request.Data?.Any() == true)
            {
                foreach (var kvp in request.Data)
                {
                    messageData[kvp.Key] = kvp.Value;
                }
            }

            messageData["platform"] = "web";
            messageData["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            messageData["clickAction"] = GetActionUrl(request.Data);

            message.Data = messageData;

            return message;
        }

        // Helper method to build web-optimized message
        /*   private Message BuildWebOptimizedMessage(NotificationRequest request)
           {
               var message = new Message()
               {
                   Notification = new Notification()
                   {
                       Title = request.Title,
                       Body = request.Body,
                       ImageUrl = request.ImageUrl
                   },
                   Webpush = new WebpushConfig()
                   {
                       Headers = new Dictionary<string, string>
                       {
                           ["TTL"] = "86400",
                           ["Urgency"] = "high"
                       },
                       Notification = new WebpushNotification()
                       {
                           Title = request.Title,
                           Body = request.Body,
                           Icon = request.ImageUrl ?? "/icons/notification-icon.png",
                           Badge = "/icons/badge-icon.png",
                           Image = request.ImageUrl,
                           RequireInteraction = true,
                           Silent = false,
                           Tag = "sabimarket-notification",
                           Dir = "ltr",
                           Lang = "en",
                           Vibrate = new[] { 200, 100, 200 },
                           Actions = new[]
                           {
                       new WebpushNotificationAction()
                       {
                           Action = "view",
                           Title = "View Details",
                           Icon = "/icons/view-icon.png"
                       },
                       new WebpushNotificationAction()
                       {
                           Action = "dismiss",
                           Title = "Dismiss",
                           Icon = "/icons/dismiss-icon.png"
                       }
                   }
                       },
                       FcmOptions = new WebpushFcmOptions()
                       {
                           Link = GetActionUrl(request.Data)
                       }
                   }
               };

               // Enhanced data for web clients
               message.Data = request.Data ?? new Dictionary<string, string>();
               message.Data["platform"] = "web";
               message.Data["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
               message.Data["clickAction"] = GetActionUrl(request.Data);

               return message;
           }
   */
        // Helper method to get tokens by device type
        private async Task<List<string>> GetTokensByDeviceType(List<string> tokens, string deviceType)
        {
            try
            {
                return await _context.DeviceTokens
                    .Where(dt => tokens.Contains(dt.Token) && dt.DeviceType == deviceType && dt.IsActive)
                    .Select(dt => dt.Token)
                    .ToListAsync();
            }
            catch
            {
                return new List<string>();
            }
        }
        public async Task<NotificationResponse> SendToUserAsync(string userId, NotificationRequest request)
        {
            var userTokens = await _context.DeviceTokens
                .Where(dt => dt.UserId == userId && dt.IsActive)
                .Select(dt => dt.Token)
                .ToListAsync();

            if (!userTokens.Any())
            {
                _logger.LogWarning("No active device tokens found for user {UserId}", userId);
                return new NotificationResponse
                {
                    Success = false,
                    Message = "No active device tokens found for user"
                };
            }

            // Save notification to database
            var notification = new UserNotification
            {
                UserId = userId,
                Title = request.Title,
                Body = request.Body,
                ImageUrl = request.ImageUrl,
                DataJson = request.Data != null ? JsonConvert.SerializeObject(request.Data) : null,
                Type = NotificationType.General
            };

            _context.UserNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send push notification
            request.Tokens = userTokens;
            return await SendNotificationAsync(request);
        }

        public async Task<NotificationResponse> SendToMultipleUsersAsync(List<string> userIds, NotificationRequest request)
        {
            var allTokens = new List<string>();
            var notifications = new List<UserNotification>();

            foreach (var userId in userIds)
            {
                var userTokens = await _context.DeviceTokens
                    .Where(dt => dt.UserId == userId && dt.IsActive)
                    .Select(dt => dt.Token)
                    .ToListAsync();

                allTokens.AddRange(userTokens);

                notifications.Add(new UserNotification
                {
                    UserId = userId,
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl,
                    DataJson = request.Data != null ? JsonConvert.SerializeObject(request.Data) : null,
                    Type = NotificationType.General
                });
            }

            if (notifications.Any())
            {
                _context.UserNotifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }

            if (!allTokens.Any())
            {
                return new NotificationResponse
                {
                    Success = false,
                    Message = "No active device tokens found for specified users"
                };
            }

            request.Tokens = allTokens.Distinct().ToList();
            return await SendNotificationAsync(request);
        }

        public async Task<NotificationResponse> SendToTopicAsync(string topic, NotificationRequest request)
        {
            request.Topic = topic;
            return await SendNotificationAsync(request);
        }

        public async Task<bool> SaveDeviceTokenToDatabase(string userId, string token, string deviceType, string? deviceInfo = null)
        {
            try
            {
                var existingToken = await _context.DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == token);

                if (existingToken != null)
                {
                    existingToken.LastUsed = DateTime.UtcNow;
                    existingToken.IsActive = true;
                    existingToken.DeviceInfo = deviceInfo;
                }
                else
                {
                    var deviceToken = new DeviceToken
                    {
                        UserId = userId,
                        Token = token,
                        DeviceType = deviceType,
                        DeviceInfo = deviceInfo,
                        IsActive = true
                    };

                    _context.DeviceTokens.Add(deviceToken);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Device token registered for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> RemoveDeviceTokenAsync(string userId, string token)
        {
            try
            {
                var deviceToken = await _context.DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == token);

                if (deviceToken != null)
                {
                    deviceToken.IsActive = false;
                    deviceToken.Token = token;

                    _context.Remove(deviceToken);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Device token removed for user {UserId}", userId);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing device token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> CleanupInactiveTokensAsync(int daysThreshold = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);
                var inactiveTokens = await _context.DeviceTokens
                    .Where(dt => dt.LastUsed < cutoffDate && dt.IsActive)
                    .ToListAsync();

                foreach (var token in inactiveTokens)
                {
                    token.IsActive = false;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} inactive device tokens", inactiveTokens.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up inactive tokens");
                return false;
            }
        }

        public async Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            var notification = await _context.UserNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            var unreadNotifications = await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _context.UserNotifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId, string userId)
        {
            var notification = await _context.UserNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                _context.UserNotifications.Remove(notification);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // Business-specific methods for SabiMarket
        public async Task<bool> SendOrderNotificationAsync(string userId, string orderId, NotificationType type)
        {
            var (title, body) = GetOrderNotificationContent(type, orderId);

            var request = new NotificationRequest
            {
                Title = title,
                Body = body,
                Data = new Dictionary<string, string>
                {
                    ["orderId"] = orderId,
                    ["type"] = type.ToString(),
                    ["actionUrl"] = $"/orders/{orderId}"
                }
            };

            var result = await SendToUserAsync(userId, request);
            return result.Success;
        }

        public async Task<bool> SendProductNotificationAsync(List<string> userIds, string productId, string message)
        {
            var request = new NotificationRequest
            {
                Title = "Product Update",
                Body = message,
                Data = new Dictionary<string, string>
                {
                    ["productId"] = productId,
                    ["type"] = "product_update",
                    ["actionUrl"] = $"/products/{productId}"
                }
            };

            var result = await SendToMultipleUsersAsync(userIds, request);
            return result.Success;
        }

        public async Task<bool> SendPromotionalNotificationAsync(string title, string body, string? imageUrl = null)
        {
            var request = new NotificationRequest
            {
                Title = title,
                Body = body,
                ImageUrl = imageUrl,
                Topic = "promotions"
            };

            var result = await SendToTopicAsync("promotions", request);
            return result.Success;
        }

        private async Task CleanupInvalidTokens(List<string> tokens, BatchResponse response)
        {
            try
            {
                var invalidTokens = new List<string>();

                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var resp = response.Responses[i];
                    if (!resp.IsSuccess && resp.Exception != null)
                    {
                        // Fix: Check the exception message instead of comparing ErrorCode directly
                        var exceptionMessage = resp.Exception.Message?.ToLower();
                        if (exceptionMessage != null &&
                            (exceptionMessage.Contains("registration-token-not-registered") ||
                             exceptionMessage.Contains("invalid-registration-token") ||
                             exceptionMessage.Contains("requested entity was not found") ||
                             exceptionMessage.Contains("invalid argument")))
                        {
                            invalidTokens.Add(tokens[i]);
                        }
                    }
                }

                if (invalidTokens.Any())
                {
                    var tokensToDeactivate = await _context.DeviceTokens
                        .Where(dt => invalidTokens.Contains(dt.Token))
                        .ToListAsync();

                    foreach (var token in tokensToDeactivate)
                    {
                        token.IsActive = false;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Deactivated {Count} invalid tokens", invalidTokens.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up invalid tokens");
            }
        }

        private List<string> GetFailedTokens(List<string> tokens, BatchResponse response)
        {
            var failedTokens = new List<string>();

            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (!response.Responses[i].IsSuccess)
                {
                    failedTokens.Add(tokens[i]);
                }
            }

            return failedTokens;
        }

        private (string title, string body) GetOrderNotificationContent(NotificationType type, string orderId)
        {
            return type switch
            {
                NotificationType.OrderPlaced => ("Order Confirmed", $"Your order #{orderId} has been placed successfully!"),
                NotificationType.OrderShipped => ("Order Shipped", $"Your order #{orderId} is on its way!"),
                NotificationType.OrderDelivered => ("Order Delivered", $"Your order #{orderId} has been delivered!"),
                _ => ("Order Update", $"Update for your order #{orderId}")
            };
        }

        // Replace your RegisterWebTokenAsync method with this unified method
        public async Task<bool> RegisterDeviceTokenAsync(string userId, string token, string? userAgent = null, string? platform = null)
        {
            try
            {
                string deviceType;
                string deviceInfo;

                // Auto-detect platform if not provided
                if (string.IsNullOrEmpty(platform))
                {
                    platform = DetectPlatformFromUserAgent(userAgent);
                }

                // Determine device type and info based on platform and user agent
                switch (platform?.ToLower())
                {
                    case "web":
                    case "browser":
                        deviceType = "Web";
                        deviceInfo = GetBrowserInfo(userAgent);
                        break;

                    case "android":
                        deviceType = "Android";
                        deviceInfo = GetAndroidInfo(userAgent);
                        break;

                    case "ios":
                    case "iphone":
                    case "ipad":
                        deviceType = "iOS";
                        deviceInfo = GetIOSInfo(userAgent);
                        break;

                    case "mobile":
                        // Generic mobile - try to detect specific platform
                        if (userAgent?.Contains("Android", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            deviceType = "Android";
                            deviceInfo = GetAndroidInfo(userAgent);
                        }
                        else if (userAgent?.Contains("iPhone", StringComparison.OrdinalIgnoreCase) == true ||
                                 userAgent?.Contains("iPad", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            deviceType = "iOS";
                            deviceInfo = GetIOSInfo(userAgent);
                        }
                        else
                        {
                            deviceType = "Mobile";
                            deviceInfo = "Mobile Device";
                        }
                        break;

                    default:
                        // Fallback: try to auto-detect from user agent
                        var detectedPlatform = DetectPlatformFromUserAgent(userAgent);
                        if (detectedPlatform == "Web")
                        {
                            deviceType = "Web";
                            deviceInfo = GetBrowserInfo(userAgent);
                        }
                        else
                        {
                            deviceType = "Unknown";
                            deviceInfo = "Unknown Device";
                        }
                        break;
                }

                return await SaveDeviceTokenToDatabase(userId, token, deviceType, deviceInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token for user {UserId}, platform: {Platform}", userId, platform);
                return false;
            }
        }

        // Helper method to detect platform from user agent
        private string DetectPlatformFromUserAgent(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            // Check for mobile platforms first
            if (userAgent.Contains("android"))
                return "Android";

            if (userAgent.Contains("iphone") || userAgent.Contains("ipad") || userAgent.Contains("ipod"))
                return "iOS";

            // Check for web browsers
            if (userAgent.Contains("chrome") || userAgent.Contains("firefox") ||
                userAgent.Contains("safari") || userAgent.Contains("edge") ||
                userAgent.Contains("opera") || userAgent.Contains("mozilla"))
                return "Web";

            return "Unknown";
        }

        // Helper method to get browser information
        private string GetBrowserInfo(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Web Browser";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("edg/")) // Microsoft Edge (Chromium)
                return "Microsoft Edge";
            else if (userAgent.Contains("chrome/") && !userAgent.Contains("edg/"))
                return "Google Chrome";
            else if (userAgent.Contains("firefox/"))
                return "Mozilla Firefox";
            else if (userAgent.Contains("safari/") && !userAgent.Contains("chrome/"))
                return "Safari";
            else if (userAgent.Contains("opera/") || userAgent.Contains("opr/"))
                return "Opera";
            else if (userAgent.Contains("ie/") || userAgent.Contains("trident/"))
                return "Internet Explorer";
            else if (userAgent.Contains("brave/") || userAgent.Contains("br/"))
                return "Brave";
            else
                return "Web Browser";
        }

        // Helper method to get Android device information
        private string GetAndroidInfo(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Android Device";

            try
            {
                // Extract Android version
                var androidMatch = System.Text.RegularExpressions.Regex.Match(userAgent, @"Android (\d+\.?\d*)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (androidMatch.Success)
                {
                    var version = androidMatch.Groups[1].Value;
                    return $"Android {version}";
                }

                return "Android Device";
            }
            catch
            {
                return "Android Device";
            }
        }

        // Helper method to get iOS device information
        private string GetIOSInfo(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "iOS Device";

            try
            {
                // Extract iOS version and device type
                var iosMatch = System.Text.RegularExpressions.Regex.Match(userAgent, @"OS (\d+_?\d*_?\d*)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                string deviceType = "iOS";
                if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
                    deviceType = "iPhone";
                else if (userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
                    deviceType = "iPad";
                else if (userAgent.Contains("iPod", StringComparison.OrdinalIgnoreCase))
                    deviceType = "iPod";

                if (iosMatch.Success)
                {
                    var version = iosMatch.Groups[1].Value.Replace("_", ".");
                    return $"{deviceType} iOS {version}";
                }

                return $"{deviceType} Device";
            }
            catch
            {
                return "iOS Device";
            }
        }

    }
}




