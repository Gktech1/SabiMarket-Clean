namespace SabiMarket.Application.DTOs.Responses
{
    public class ChairmanResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string MarketId { get; set; } = string.Empty;
        public string LocalGovernmentId { get; set; } = string.Empty;
        public string LocalGovernmentName { get; set; } = string.Empty; // Added property
        public string MarketName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string DefaultPassword { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AdminDashboardResponse
    {
        public PaginatorDto<IEnumerable<ChairmanResponseDto>> Chairmen { get; set; }
        public DashboardMetrics Metrics { get; set; }
    }

    public class DashboardMetrics
    {
        public int RegisteredLGAs { get; set; }
        public int ActiveChairmen { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
