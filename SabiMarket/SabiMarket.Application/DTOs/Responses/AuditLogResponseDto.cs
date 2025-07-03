namespace SabiMarket.Application.DTOs.Responses
{
    public class AuditLogResponseDto
    {
        public string Id { get; set; }

        // User Information
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public string UserRole { get; set; }

        // Timing Information
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public DateTime Timestamp { get; set; }

        // Activity Details
        public string Activity { get; set; }
        public string Module { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }

        // Metadata
        public string Status { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string Location { get; set; }

        // Additional Properties for UI Display
        public string FormattedDate => Date.ToString("MMM dd, yyyy");
        public string FormattedTime => Time;
        public string FormattedTimestamp => Timestamp.ToString("MMM dd, yyyy HH:mm:ss");
        public string UserInitials => string.Join("", UserFullName?.Split(' ').Select(x => x[0]));
    }
}

