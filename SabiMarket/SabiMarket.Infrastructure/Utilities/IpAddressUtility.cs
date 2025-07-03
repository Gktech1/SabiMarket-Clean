using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using System.Net.NetworkInformation;

namespace SabiMarket.Infrastructure.Utilities
{
    public static class IpAddressUtility
    {
        /// <summary>
        /// Gets the client IP address from HttpContext with various header checks
        /// </summary>
        public static string GetClientIpAddress(HttpContext context)
        {
            try
            {
                // Check for X-Forwarded-For header
                string ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(ip))
                {
                    // Get the first IP if multiple are present
                    return ip.Split(',')[0].Trim();
                }

                // Check for X-Real-IP header
                ip = context.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(ip))
                {
                    return ip;
                }

                // Get connection remote IP address
                ip = context.Connection.RemoteIpAddress?.ToString();
                if (!string.IsNullOrEmpty(ip) && ip != "::1")
                {
                    // If it's an IPv6 address, try to get the embedded IPv4 address
                    if (IPAddress.TryParse(ip, out IPAddress ipAddress) &&
                        ipAddress.IsIPv4MappedToIPv6)
                    {
                        return ipAddress.MapToIPv4().ToString();
                    }
                    return ip;
                }

                // Fallback to local IP
                return GetLocalIPAddress();
            }
            catch (Exception ex)
            {
                // Log the error and return a fallback value
                return "0.0.0.0";
            }
        }

        /// <summary>
        /// Gets the local machine's IP address
        /// </summary>
        public static string GetLocalIPAddress()
        {
            var localIPs = new List<string>();

            // Get all network interfaces
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                           n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var networkInterface in networkInterfaces)
            {
                var properties = networkInterface.GetIPProperties();

                // Get IPv4 addresses
                var ipv4Address = properties.UnicastAddresses
                    .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork &&
                                !IPAddress.IsLoopback(ua.Address))
                    .Select(ua => ua.Address.ToString())
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(ipv4Address))
                {
                    localIPs.Add(ipv4Address);
                }
            }

            // Return the first non-loopback IPv4 address, or loopback if nothing else is found
            return localIPs.FirstOrDefault() ?? "127.0.0.1";
        }

        /// <summary>
        /// Gets public IP address by querying external service
        /// </summary>
        public static async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Try multiple IP lookup services
                    var services = new[]
                    {
                        "https://api.ipify.org",
                        "https://icanhazip.com",
                        "https://ifconfig.me/ip"
                    };

                    foreach (var service in services)
                    {
                        try
                        {
                            var response = await client.GetStringAsync(service);
                            var ip = response.Trim();

                            // Validate the returned IP
                            if (IPAddress.TryParse(ip, out _))
                            {
                                return ip;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    // If all services fail, return local IP
                    return GetLocalIPAddress();
                }
            }
            catch (Exception)
            {
                return GetLocalIPAddress();
            }
        }

        /// <summary>
        /// Validates an IP address string
        /// </summary>
        public static bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            // Check if it's a valid IPv4 or IPv6 address
            return IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// Determines if an IP address is private
        /// </summary>
        public static bool IsPrivateIPAddress(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out IPAddress address))
                return false;

            byte[] bytes = address.GetAddressBytes();

            // Check for IPv4 private ranges
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                // 10.0.0.0/8
                if (bytes[0] == 10)
                    return true;

                // 172.16.0.0/12
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    return true;

                // 192.168.0.0/16
                if (bytes[0] == 192 && bytes[1] == 168)
                    return true;
            }

            return false;
        }
    }

    // Extension method for IHttpContextAccessor
    public static class HttpContextExtensions
    {
        public static string GetRemoteIPAddress(this IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext == null)
                return $"{IpAddressUtility.GetLocalIPAddress()}";

            return IpAddressUtility.GetClientIpAddress(httpContextAccessor.HttpContext);
        }
    }
}