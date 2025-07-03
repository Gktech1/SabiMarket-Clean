using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Application.DTOs.Advertisement;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Utilities;

namespace SabiMarket.Infrastructure.Repositories
{
    public class AdvertisementRepository : GeneralRepository<Advertisement>, IAdvertisementRepository
    {
        private readonly ApplicationDbContext _repositoryContext;

        public AdvertisementRepository(ApplicationDbContext repositoryContext)
            : base(repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        // EXISTING METHODS (unchanged)
        public async Task<Advertisement> GetAdvertisementById(string id, bool trackChanges) =>
            await FindByCondition(a => a.Id == id, trackChanges)
                .FirstOrDefaultAsync();

        public async Task<Advertisement> GetAdvertisementWithVendor(string id, bool trackChanges) =>
            await FindByCondition(a => a.Id == id, trackChanges)
                .Include(a => a.Vendor)
                .Include(a => a.Vendor.User)
                .FirstOrDefaultAsync();

        public async Task<Advertisement> GetAdvertisementByVendorId(string vendorId, bool trackChanges) =>
            await FindByCondition(a => a.VendorId == vendorId, trackChanges)
                .FirstOrDefaultAsync();

        public async Task<Advertisement> GetAdvertisementDetails(string id)
        {
            var advertisement = await FindByCondition(a => a.Id == id, trackChanges: false)
                .Include(a => a.Vendor)
                .Include(a => a.Admin)
                .Include(a => a.Views)
                .Include(a => a.Translations)
                .Include(a => a.Payment)
                .FirstOrDefaultAsync();

            return advertisement;
        }

        public async Task<PaginatorDto<IEnumerable<Advertisement>>> GetAdvertisementsWithPagination(
            PaginationFilter paginationFilter, bool trackChanges)
        {
            var query = FindAll(trackChanges)
                .Include(a => a.Vendor)
                .Include(a => a.Views)
                .OrderByDescending(a => a.CreatedAt);

            return await query.Paginate(paginationFilter);
        }

        public async Task<PaginatorDto<IEnumerable<Advertisement>>> GetFilteredAdvertisements(
            AdvertisementFilterRequestDto filterDto, string vendorId, PaginationFilter paginationFilter)
        {
            // Start with base query
            var query = FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .Include(a => a.Views)
                .AsQueryable();

            // Apply vendor filter if specified
            if (!string.IsNullOrEmpty(vendorId))
            {
                query = query.Where(a => a.VendorId == vendorId);
            }

            // Apply search term filter
            if (!string.IsNullOrEmpty(filterDto.SearchTerm))
            {
                var searchTerm = filterDto.SearchTerm.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(searchTerm) ||
                    a.Description.ToLower().Contains(searchTerm));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(filterDto.Status))
            {
                var statusList = filterDto.Status.Split(',');
                query = query.Where(a => statusList.Contains(a.Status.ToString()));
            }

            // Apply location filter
            if (!string.IsNullOrEmpty(filterDto.Location))
            {
                query = query.Where(a => a.Location == filterDto.Location);
            }

            // Apply language filter
            if (!string.IsNullOrEmpty(filterDto.Language))
            {
                query = query.Where(a => a.Language == filterDto.Language);
            }

            // Apply placement filter
            if (!string.IsNullOrEmpty(filterDto.AdvertPlacement))
            {
                query = query.Where(a => a.AdvertPlacement == filterDto.AdvertPlacement);
            }

            // Apply date filters
            if (filterDto.StartDateFrom.HasValue)
            {
                query = query.Where(a => a.StartDate >= filterDto.StartDateFrom.Value);
            }

            if (filterDto.StartDateTo.HasValue)
            {
                query = query.Where(a => a.StartDate <= filterDto.StartDateTo.Value);
            }

            if (filterDto.EndDateFrom.HasValue)
            {
                query = query.Where(a => a.EndDate >= filterDto.EndDateFrom.Value);
            }

            if (filterDto.EndDateTo.HasValue)
            {
                query = query.Where(a => a.EndDate <= filterDto.EndDateTo.Value);
            }

            // Order by creation date, newest first
            query = query.OrderByDescending(a => a.CreatedAt);

            // Execute pagination
            return await query.Paginate(paginationFilter);
        }

        public async Task<bool> AdvertisementExists(string id) =>
            await FindByCondition(a => a.Id == id, trackChanges: false)
                .AnyAsync();

        public void CreateAdvertisement(Advertisement advertisement) =>
            Create(advertisement);

        public void UpdateAdvertisement(Advertisement advertisement) =>
            Update(advertisement);

        public void DeleteAdvertisement(Advertisement advertisement) =>
            Delete(advertisement);

        // NEW MISSING METHODS FOR ADMIN FEATURES

        public async Task<AdvertisementAnalyticsDto> GetAdvertisementAnalyticsAsync(AnalyticsFilterDto filter)
        {
            var query = FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .ThenInclude(v => v.User)
                .Include(a => a.Views)
                .AsQueryable();

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.Location))
                query = query.Where(a => a.Location == filter.Location);

            if (!string.IsNullOrEmpty(filter.Placement))
                query = query.Where(a => a.AdvertPlacement == filter.Placement);

            if (!string.IsNullOrEmpty(filter.VendorId))
                query = query.Where(a => a.VendorId == filter.VendorId);

            if (!string.IsNullOrEmpty(filter.Status))
            {
                var statusList = filter.Status.Split(',');
                query = query.Where(a => statusList.Contains(a.Status.ToString()));
            }

            var advertisements = await query.ToListAsync();

            var analytics = new AdvertisementAnalyticsDto
            {
                TotalAdvertisements = advertisements.Count,
                ActiveAdvertisements = advertisements.Count(a => a.Status == AdvertStatusEnum.Active),
                PendingAdvertisements = advertisements.Count(a => a.Status == AdvertStatusEnum.Pending),
                RejectedAdvertisements = advertisements.Count(a => a.Status == AdvertStatusEnum.Rejected),
                ArchivedAdvertisements = advertisements.Count(a => a.Status == AdvertStatusEnum.Archived),
                TotalRevenue = advertisements.Where(a => a.PaymentStatus == "Verified").Sum(a => a.Price),
                TotalViews = advertisements.Sum(a => a.Views?.Count ?? 0),
                AverageViewsPerAd = advertisements.Count > 0 ?
                    advertisements.Average(a => a.Views?.Count ?? 0) : 0,
                AdvertisementsByLocation = advertisements
                    .GroupBy(a => a.Location)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                AdvertisementsByPlacement = advertisements
                    .GroupBy(a => a.AdvertPlacement)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                RevenueByLocation = advertisements
                    .Where(a => a.PaymentStatus == "Verified")
                    .GroupBy(a => a.Location)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Sum(a => a.Price))
            };

            // Calculate time-based revenue
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var weekStart = now.AddDays(-(int)now.DayOfWeek);
            var dayStart = now.Date;

            analytics.MonthlyRevenue = advertisements
                .Where(a => a.PaymentStatus == "Verified" && a.CreatedAt >= monthStart)
                .Sum(a => a.Price);

            analytics.WeeklyRevenue = advertisements
                .Where(a => a.PaymentStatus == "Verified" && a.CreatedAt >= weekStart)
                .Sum(a => a.Price);

            analytics.DailyRevenue = advertisements
                .Where(a => a.PaymentStatus == "Verified" && a.CreatedAt >= dayStart)
                .Sum(a => a.Price);

            // Top performing ads
            analytics.TopPerformingAds = advertisements
                .Where(a => a.Views?.Any() == true)
                .OrderByDescending(a => a.Views.Count)
                .Take(10)
                .Select(a => new TopPerformingAdDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    VendorName = $"{a.Vendor?.User?.FirstName} {a.Vendor?.User?.LastName}".Trim(),
                    Views = a.Views?.Count ?? 0,
                    Revenue = a.PaymentStatus == "Verified" ? a.Price : 0,
                    Location = a.Location,
                    Placement = a.AdvertPlacement
                }).ToList();

            // Daily stats for the last 30 days
            analytics.DailyStats = advertisements
                .Where(a => a.CreatedAt >= now.AddDays(-30))
                .GroupBy(a => a.CreatedAt.Date)
                .Select(g => new DailyStatsDto
                {
                    Date = g.Key,
                    NewAdvertisements = g.Count(),
                    Approvals = g.Count(a => a.Status == AdvertStatusEnum.Active),
                    Rejections = g.Count(a => a.Status == AdvertStatusEnum.Rejected),
                    Revenue = g.Where(a => a.PaymentStatus == "Verified").Sum(a => a.Price),
                    TotalViews = g.Sum(a => a.Views?.Count ?? 0)
                }).OrderBy(d => d.Date).ToList();

            return analytics;
        }

        public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .ThenInclude(v => v.User)
                .Where(a => a.PaymentStatus == "Verified")
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var advertisements = await query.ToListAsync();

            var revenue = new RevenueAnalyticsDto
            {
                TotalRevenue = advertisements.Sum(a => a.Price),
                AverageOrderValue = advertisements.Count > 0 ? advertisements.Average(a => a.Price) : 0,
                RevenueByLocation = advertisements
                    .GroupBy(a => a.Location)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Sum(a => a.Price)),
                RevenueByPlacement = advertisements
                    .GroupBy(a => a.AdvertPlacement)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Sum(a => a.Price))
            };

            // Monthly breakdown
            revenue.MonthlyBreakdown = advertisements
                .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(a => a.Price),
                    AdCount = g.Count()
                }).OrderBy(m => m.Month).ToList();

            // Calculate growth
            if (revenue.MonthlyBreakdown.Count > 1)
            {
                var currentMonth = revenue.MonthlyBreakdown.Last();
                var previousMonth = revenue.MonthlyBreakdown[revenue.MonthlyBreakdown.Count - 2];

                if (previousMonth.Revenue > 0)
                {
                    revenue.MonthlyGrowth = ((currentMonth.Revenue - previousMonth.Revenue) / previousMonth.Revenue) * 100;
                }
            }

            // Top vendors by revenue
            revenue.TopVendors = advertisements
                .GroupBy(a => a.VendorId)
                .Select(g => new TopVendorRevenueDto
                {
                    VendorId = g.Key,
                    VendorName = $"{g.First().Vendor?.User?.FirstName} {g.First().Vendor?.User?.LastName}".Trim(),
                    VendorEmail = g.First().Vendor?.User?.Email,
                    TotalRevenue = g.Sum(a => a.Price),
                    AdCount = g.Count(),
                    AverageAdValue = g.Average(a => a.Price)
                })
                .OrderByDescending(v => v.TotalRevenue)
                .Take(10)
                .ToList();

            return revenue;
        }

        public async Task<AdvertisementDashboardStatsDto> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var allAds = await FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .ThenInclude(v => v.User)
                .Include(a => a.Views)
                .ToListAsync();

            var stats = new AdvertisementDashboardStatsDto
            {
                TotalAdvertisements = allAds.Count,
                TodayAdvertisements = allAds.Count(a => a.CreatedAt.Date == today),
                WeekAdvertisements = allAds.Count(a => a.CreatedAt >= weekStart),
                MonthAdvertisements = allAds.Count(a => a.CreatedAt >= monthStart),
                PendingApprovals = allAds.Count(a => a.Status == AdvertStatusEnum.Pending),
                PendingPayments = allAds.Count(a => a.PaymentStatus == "Pending Verification"),
                TotalRevenue = allAds.Where(a => a.PaymentStatus == "Verified").Sum(a => a.Price),
                MonthlyRevenue = allAds.Where(a => a.PaymentStatus == "Verified" && a.CreatedAt >= monthStart).Sum(a => a.Price),
                WeeklyRevenue = allAds.Where(a => a.PaymentStatus == "Verified" && a.CreatedAt >= weekStart).Sum(a => a.Price),
                DailyRevenue = allAds.Where(a => a.PaymentStatus == "Verified" && a.CreatedAt.Date == today).Sum(a => a.Price),
                ActiveVendors = allAds.Where(a => a.Vendor?.User?.IsActive == true).Select(a => a.VendorId).Distinct().Count(),
                TotalViews = allAds.Sum(a => a.Views?.Count ?? 0),
                StatusBreakdown = allAds.GroupBy(a => a.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
                RevenueByLocation = allAds.Where(a => a.PaymentStatus == "Verified")
                    .GroupBy(a => a.Location).ToDictionary(g => g.Key ?? "Unknown", g => g.Sum(a => a.Price))
            };

            // Recent activities
            stats.RecentActivities = allAds
                .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
                .Take(10)
                .Select(a => new RecentActivityDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Activity = GetActivityType(a),
                    Description = $"{a.Title} - {a.Status}",
                    UserName = $"{a.Vendor?.User?.FirstName} {a.Vendor?.User?.LastName}".Trim(),
                    Timestamp = a.UpdatedAt ?? a.CreatedAt,
                    Type = a.Status.ToString(),
                    AdvertisementId = a.Id,
                    AdvertisementTitle = a.Title
                }).ToList();

            return stats;
        }

        private string GetActivityType(Advertisement advertisement)
        {
            if (advertisement.UpdatedAt.HasValue && advertisement.UpdatedAt > advertisement.CreatedAt.AddMinutes(1))
            {
                return advertisement.Status switch
                {
                    AdvertStatusEnum.Active => "Advertisement Approved",
                    AdvertStatusEnum.Rejected => "Advertisement Rejected",
                    AdvertStatusEnum.Archived => "Advertisement Archived",
                    _ => "Advertisement Updated"
                };
            }
            return "Advertisement Created";
        }

        public async Task<List<Advertisement>> GetAdvertisementsForExportAsync(DateTime? startDate, DateTime? endDate,
            string status, string location, string vendorId)
        {
            var query = FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .ThenInclude(v => v.User)
                .Include(a => a.Views)
                .Include(a => a.Payment)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            if (!string.IsNullOrEmpty(status))
            {
                var statusList = status.Split(',');
                query = query.Where(a => statusList.Contains(a.Status.ToString()));
            }

            if (!string.IsNullOrEmpty(location))
                query = query.Where(a => a.Location == location);

            if (!string.IsNullOrEmpty(vendorId))
                query = query.Where(a => a.VendorId == vendorId);

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task<PaginatorDto<IEnumerable<VendorAdvertisementSummaryDto>>> GetVendorAdvertisementSummariesAsync(
            VendorFilterDto filter, PaginationFilter paginationFilter)
        {
            // Get all vendors with their advertisements
            var vendorQuery = _repositoryContext.Vendors
                .Include(v => v.User)
                .Include(v => v.Advertisements)
                .ThenInclude(a => a.Views)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                vendorQuery = vendorQuery.Where(v =>
                    (v.User.FirstName + " " + v.User.LastName).ToLower().Contains(searchTerm) ||
                    v.User.Email.ToLower().Contains(searchTerm) ||
                    (v.User.PhoneNumber != null && v.User.PhoneNumber.Contains(searchTerm)));
            }

            if (filter.IsActive.HasValue)
                vendorQuery = vendorQuery.Where(v => v.User.IsActive == filter.IsActive.Value);

            if (filter.JoinedFrom.HasValue)
                vendorQuery = vendorQuery.Where(v => v.CreatedAt >= filter.JoinedFrom.Value);

            if (filter.JoinedTo.HasValue)
                vendorQuery = vendorQuery.Where(v => v.CreatedAt <= filter.JoinedTo.Value);

            var vendors = await vendorQuery.ToListAsync();

            // Calculate summaries
            var summaries = vendors.Select(v =>
            {
                var totalAds = v.Advertisements?.Count ?? 0;
                var activeAds = v.Advertisements?.Count(a => a.Status == AdvertStatusEnum.Active) ?? 0;
                var pendingAds = v.Advertisements?.Count(a => a.Status == AdvertStatusEnum.Pending) ?? 0;
                var rejectedAds = v.Advertisements?.Count(a => a.Status == AdvertStatusEnum.Rejected) ?? 0;
                var totalRevenue = v.Advertisements?.Where(a => a.PaymentStatus == "Verified").Sum(a => a.Price) ?? 0;

                return new VendorAdvertisementSummaryDto
                {
                    VendorId = v.Id,
                    VendorName = $"{v.User?.FirstName} {v.User?.LastName}".Trim(),
                    VendorEmail = v.User?.Email,
                    VendorPhone = v.User?.PhoneNumber,
                    TotalAdvertisements = totalAds,
                    ActiveAdvertisements = activeAds,
                    PendingAdvertisements = pendingAds,
                    RejectedAdvertisements = rejectedAds,
                    TotalRevenue = totalRevenue,
                    AverageAdValue = totalAds > 0 ? totalRevenue / totalAds : 0,
                    LastAdCreated = v.Advertisements?.OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue,
                    VendorJoinDate = v.CreatedAt,
                    IsActive = v.User?.IsActive ?? false,
                    SuccessRate = totalAds > 0 ? (double)activeAds / totalAds * 100 : 0,
                    RecentAdvertisements = v.Advertisements?.OrderByDescending(a => a.CreatedAt).Take(3)
                        .Select(a => new AdvertisementSummaryDto
                        {
                            Id = a.Id,
                            Title = a.Title,
                            Status = a.Status.ToString(),
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            Price = a.Price,
                            ViewCount = a.Views?.Count ?? 0,
                            CreatedAt = a.CreatedAt
                        }).ToList() ?? new List<AdvertisementSummaryDto>()
                };
            }).AsQueryable();

            // Apply additional filters on calculated fields
            if (filter.MinRevenue.HasValue)
                summaries = summaries.Where(s => s.TotalRevenue >= filter.MinRevenue.Value);

            if (filter.MaxRevenue.HasValue)
                summaries = summaries.Where(s => s.TotalRevenue <= filter.MaxRevenue.Value);

            if (filter.MinAds.HasValue)
                summaries = summaries.Where(s => s.TotalAdvertisements >= filter.MinAds.Value);

            if (filter.MaxAds.HasValue)
                summaries = summaries.Where(s => s.TotalAdvertisements <= filter.MaxAds.Value);

            // Order by total revenue descending
            summaries = summaries.OrderByDescending(s => s.TotalRevenue);

            // Apply pagination manually since we're working with calculated data
            var totalCount = summaries.Count();
            var pagedSummaries = summaries
                .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
                .Take(paginationFilter.PageSize)
                .ToList();

            var numberOfPages = (int)Math.Ceiling(totalCount / (double)paginationFilter.PageSize);

            return new PaginatorDto<IEnumerable<VendorAdvertisementSummaryDto>>
            {
                PageItems = pagedSummaries,
                PageSize = paginationFilter.PageSize,
                CurrentPage = paginationFilter.PageNumber,
                NumberOfPages = numberOfPages
            };
        }

        public async Task<PaginatorDto<IEnumerable<PaymentVerificationDto>>> GetPendingPaymentVerificationsAsync(
            PaginationFilter paginationFilter)
        {
            var query = FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .ThenInclude(v => v.User)
                .Include(a => a.Payment)
                .Where(a => a.PaymentStatus == "Pending Verification" && !string.IsNullOrEmpty(a.PaymentProofUrl))
                .OrderBy(a => a.UpdatedAt ?? a.CreatedAt);

            var paginatedResult = await query.Paginate(paginationFilter);

            var verificationDtos = paginatedResult.PageItems.Select(a => new PaymentVerificationDto
            {
                Id = a.Payment?.Id ?? Guid.NewGuid().ToString(),
                AdvertisementId = a.Id,
                AdvertisementTitle = a.Title,
                VendorName = $"{a.Vendor?.User?.FirstName} {a.Vendor?.User?.LastName}".Trim(),
                ActionRequired = "Verify Payment Proof",
                ActionUrl = $"/admin/payments/verify/{a.Id}"
            });

            return new PaginatorDto<IEnumerable<PaymentVerificationDto>>
            {
                PageItems = verificationDtos,
                PageSize = paginatedResult.PageSize,
                CurrentPage = paginatedResult.CurrentPage,
                NumberOfPages = paginatedResult.NumberOfPages
            };
        }

        public async Task<PaginatorDto<IEnumerable<Advertisement>>> GetFilteredAdvertisementsForAdmin(
            AdminAdvertisementFilterDto filterDto, PaginationFilter paginationFilter)
        {
            var query = FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .ThenInclude(v => v.User)
                .Include(a => a.Views)
                .Include(a => a.Admin)
                .ThenInclude(admin => admin.User)
                .Include(a => a.Payment)
                .AsQueryable();

            // Apply base filters
            if (!string.IsNullOrEmpty(filterDto.SearchTerm))
            {
                var searchTerm = filterDto.SearchTerm.ToLower();
                query = query.Where(a =>
                    a.Title.ToLower().Contains(searchTerm) ||
                    a.Description.ToLower().Contains(searchTerm));
            }

            // Admin-specific filters
            if (!string.IsNullOrEmpty(filterDto.VendorId))
                query = query.Where(a => a.VendorId == filterDto.VendorId);

            if (!string.IsNullOrEmpty(filterDto.VendorName))
            {
                var vendorName = filterDto.VendorName.ToLower();
                query = query.Where(a =>
                    (a.Vendor.User.FirstName + " " + a.Vendor.User.LastName).ToLower().Contains(vendorName));
            }

            if (!string.IsNullOrEmpty(filterDto.VendorEmail))
                query = query.Where(a => a.Vendor.User.Email.ToLower().Contains(filterDto.VendorEmail.ToLower()));

            if (!string.IsNullOrEmpty(filterDto.PaymentStatus))
                query = query.Where(a => a.PaymentStatus == filterDto.PaymentStatus);

            if (filterDto.MinAmount.HasValue)
                query = query.Where(a => a.Price >= filterDto.MinAmount.Value);

            if (filterDto.MaxAmount.HasValue)
                query = query.Where(a => a.Price <= filterDto.MaxAmount.Value);

            if (filterDto.MinViews.HasValue)
                query = query.Where(a => a.Views.Count >= filterDto.MinViews.Value);

            if (filterDto.MaxViews.HasValue)
                query = query.Where(a => a.Views.Count <= filterDto.MaxViews.Value);

            if (!string.IsNullOrEmpty(filterDto.AdminId))
                query = query.Where(a => a.AdminId == filterDto.AdminId);

            if (!string.IsNullOrEmpty(filterDto.ApprovalStatus))
            {
                if (filterDto.ApprovalStatus == "Approved")
                    query = query.Where(a => a.Status == AdvertStatusEnum.Active);
                else if (filterDto.ApprovalStatus == "Rejected")
                    query = query.Where(a => a.Status == AdvertStatusEnum.Rejected);
                else if (filterDto.ApprovalStatus == "Pending")
                    query = query.Where(a => a.Status == AdvertStatusEnum.Pending);
            }

            if (filterDto.PaymentSubmittedFrom.HasValue)
                query = query.Where(a => a.UpdatedAt >= filterDto.PaymentSubmittedFrom.Value);

            if (filterDto.PaymentSubmittedTo.HasValue)
                query = query.Where(a => a.UpdatedAt <= filterDto.PaymentSubmittedTo.Value);

            // Apply other base filters
            if (!string.IsNullOrEmpty(filterDto.Status))
            {
                var statusList = filterDto.Status.Split(',');
                query = query.Where(a => statusList.Contains(a.Status.ToString()));
            }

            if (!string.IsNullOrEmpty(filterDto.Location))
                query = query.Where(a => a.Location == filterDto.Location);

            if (!string.IsNullOrEmpty(filterDto.Language))
                query = query.Where(a => a.Language == filterDto.Language);

            if (!string.IsNullOrEmpty(filterDto.AdvertPlacement))
                query = query.Where(a => a.AdvertPlacement == filterDto.AdvertPlacement);

            // Apply date filters
            if (filterDto.StartDateFrom.HasValue)
                query = query.Where(a => a.StartDate >= filterDto.StartDateFrom.Value);

            if (filterDto.StartDateTo.HasValue)
                query = query.Where(a => a.StartDate <= filterDto.StartDateTo.Value);

            if (filterDto.EndDateFrom.HasValue)
                query = query.Where(a => a.EndDate >= filterDto.EndDateFrom.Value);

            if (filterDto.EndDateTo.HasValue)
                query = query.Where(a => a.EndDate <= filterDto.EndDateTo.Value);

            query = query.OrderByDescending(a => a.CreatedAt);
            return await query.Paginate(paginationFilter);
        }

        public async Task<List<AdvertisementAlertDto>> GetAdvertisementAlertsAsync()
        {
            var alerts = new List<AdvertisementAlertDto>();
            var now = DateTime.UtcNow;

            // Get advertisements that need attention
            var advertisements = await FindAll(trackChanges: false)
                .Include(a => a.Vendor)
                .ThenInclude(v => v.User)
                .Where(a =>
                    a.Status == AdvertStatusEnum.Pending ||
                    a.PaymentStatus == "Pending Verification" ||
                    (a.Status == AdvertStatusEnum.Active && a.EndDate <= now.AddDays(7)))
                .ToListAsync();

            // Pending approvals
            var pendingApprovals = advertisements.Where(a => a.Status == AdvertStatusEnum.Pending).ToList();
            if (pendingApprovals.Any())
            {
                alerts.Add(new AdvertisementAlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "PendingApprovals",
                    Title = "Pending Approvals",
                    Message = $"{pendingApprovals.Count} advertisements waiting for approval",
                    Priority = pendingApprovals.Count > 10 ? "High" : "Medium",
                    CreatedAt = now,
                    ActionRequired = "Review and approve/reject advertisements",
                    ActionUrl = "/admin/advertisements?status=pending"
                });
            }

            // Pending payment verifications
            var pendingPayments = advertisements.Where(a => a.PaymentStatus == "Pending Verification").ToList();
            if (pendingPayments.Any())
            {
                alerts.Add(new AdvertisementAlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "PendingPayments",
                    Title = "Pending Payment Verifications",
                    Message = $"{pendingPayments.Count} payment proofs waiting for verification",
                    Priority = "High",
                    CreatedAt = now,
                    ActionRequired = "Verify payment proofs",
                    ActionUrl = "/admin/payments/pending"
                });
            }

            // Expiring advertisements
            var expiringAds = advertisements.Where(a =>
                a.Status == AdvertStatusEnum.Active &&
                a.EndDate <= now.AddDays(7) &&
                a.EndDate > now).ToList();

            if (expiringAds.Any())
            {
                alerts.Add(new AdvertisementAlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "ExpiringAdvertisements",
                    Title = "Expiring Advertisements",
                    Message = $"{expiringAds.Count} advertisements expiring within 7 days",
                    Priority = "Medium",
                    CreatedAt = now,
                    ActionRequired = "Review expiring advertisements",
                    ActionUrl = "/admin/advertisements?expiring=true"
                });
            }

            // Overdue advertisements
            var overdueAds = advertisements.Where(a =>
                a.Status == AdvertStatusEnum.Active &&
                a.EndDate < now).ToList();

            if (overdueAds.Any())
            {
                alerts.Add(new AdvertisementAlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "OverdueAdvertisements",
                    Title = "Overdue Advertisements",
                    Message = $"{overdueAds.Count} advertisements have expired and need attention",
                    Priority = "High",
                    CreatedAt = now,
                    ActionRequired = "Archive or extend expired advertisements",
                    ActionUrl = "/admin/advertisements?overdue=true"
                });
            }

            // Old pending advertisements (more than 7 days)
            var oldPendingAds = pendingApprovals.Where(a =>
                (now - a.CreatedAt).TotalDays > 7).ToList();

            if (oldPendingAds.Any())
            {
                alerts.Add(new AdvertisementAlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "OldPendingAds",
                    Title = "Old Pending Advertisements",
                    Message = $"{oldPendingAds.Count} advertisements have been pending for more than 7 days",
                    Priority = "Medium",
                    CreatedAt = now,
                    ActionRequired = "Review old pending advertisements",
                    ActionUrl = "/admin/advertisements?status=pending&old=true"
                });
            }

            // Old pending payments (more than 3 days)
            var oldPendingPayments = pendingPayments.Where(a =>
                (now - (a.UpdatedAt ?? a.CreatedAt)).TotalDays > 3).ToList();

            if (oldPendingPayments.Any())
            {
                alerts.Add(new AdvertisementAlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "OldPendingPayments",
                    Title = "Old Pending Payments",
                    Message = $"{oldPendingPayments.Count} payment verifications have been pending for more than 3 days",
                    Priority = "High",
                    CreatedAt = now,
                    ActionRequired = "Urgently verify old pending payments",
                    ActionUrl = "/admin/payments/pending?old=true"
                });
            }

            return alerts;
        }

        public async Task<AdvertisementPerformanceDto> GetAdvertisementPerformanceAsync(string advertisementId)
        {
            var advertisement = await FindByCondition(a => a.Id == advertisementId, trackChanges: false)
                .Include(a => a.Views)
                .FirstOrDefaultAsync();

            if (advertisement == null)
                return null;

            var performance = new AdvertisementPerformanceDto
            {
                AdvertisementId = advertisement.Id,
                Title = advertisement.Title,
                TotalViews = advertisement.Views?.Count ?? 0,
                UniqueViews = advertisement.Views?.GroupBy(v => v.IPAddress).Count() ?? 0,
                Clicks = 0, // You'll need to implement click tracking
                Revenue = advertisement.PaymentStatus == "Verified" ? advertisement.Price : 0,
                StartDate = advertisement.StartDate,
                EndDate = advertisement.EndDate,
                DaysActive = (int)(DateTime.UtcNow - advertisement.StartDate).TotalDays,
                Location = advertisement.Location,
                Placement = advertisement.AdvertPlacement
            };

            // Calculate performance metrics
            if (advertisement.Views?.Any() == true)
            {
                var views = advertisement.Views.ToList();

                performance.DailyPerformance = views
                    .GroupBy(v => v.ViewedAt.Date)
                    .Select(g => new DailyPerformanceDto
                    {
                        Date = g.Key,
                        Views = g.Count(),
                        Clicks = 0, // You'll need to implement click tracking
                        ClickThroughRate = 0, // Calculate based on clicks/views
                        Revenue = 0 // Revenue attribution per day
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                performance.HourlyPerformance = views
                    .GroupBy(v => v.ViewedAt.Hour)
                    .Select(g => new HourlyPerformanceDto
                    {
                        Hour = g.Key,
                        Views = g.Count(),
                        Clicks = 0,
                        ClickThroughRate = 0
                    })
                    .OrderBy(h => h.Hour)
                    .ToList();

                performance.ViewsByLocation = views
                    .Where(v => !string.IsNullOrEmpty(v.IPAddress))
                    .GroupBy(v => v.IPAddress)
                    .ToDictionary(g => "Unknown", g => g.Count()); // Placeholder - implement geolocation

                performance.ViewsByDevice = views
                    .GroupBy(v => "Desktop") // Placeholder - implement device detection
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            // Calculate rates
            if (performance.TotalViews > 0)
            {
                performance.ClickThroughRate = performance.Clicks > 0 ?
                    (double)performance.Clicks / performance.TotalViews * 100 : 0;

                performance.AverageViewsPerDay = performance.DaysActive > 0 ?
                    (double)performance.TotalViews / performance.DaysActive : performance.TotalViews;

                performance.AverageClicksPerDay = performance.DaysActive > 0 ?
                    (double)performance.Clicks / performance.DaysActive : performance.Clicks;
            }

            return performance;
        }
    }
}


/*using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories;
using SabiMarket.Infrastructure.Utilities;

public class AdvertisementRepository : GeneralRepository<Advertisement>, IAdvertisementRepository
{
    private readonly ApplicationDbContext _repositoryContext;

    public AdvertisementRepository(ApplicationDbContext repositoryContext)
        : base(repositoryContext)
    {
        _repositoryContext = repositoryContext;
    }

    public async Task<Advertisement> GetAdvertisementById(string id, bool trackChanges) =>
        await FindByCondition(a => a.Id == id, trackChanges)
            .FirstOrDefaultAsync();

    public async Task<Advertisement> GetAdvertisementWithVendor(string id, bool trackChanges) =>
        await FindByCondition(a => a.Id == id, trackChanges)
            .Include(a => a.Vendor)
            .Include(a => a.Vendor.User)
            .FirstOrDefaultAsync();

    public async Task<Advertisement> GetAdvertisementByVendorId(string vendorId, bool trackChanges) =>
        await FindByCondition(a => a.VendorId == vendorId, trackChanges)
            .FirstOrDefaultAsync();

    public async Task<Advertisement> GetAdvertisementDetails(string id)
    {
        var advertisement = await FindByCondition(a => a.Id == id, trackChanges: false)
            .Include(a => a.Vendor)
            .Include(a => a.Admin)
            .Include(a => a.Views)
            .Include(a => a.Translations)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync();
        return advertisement;
    }

    public async Task<PaginatorDto<IEnumerable<Advertisement>>> GetAdvertisementsWithPagination(
        PaginationFilter paginationFilter, bool trackChanges)
    {
        var query = FindAll(trackChanges)
            .Include(a => a.Vendor)
            .OrderByDescending(a => a.CreatedAt);
        return await query.Paginate(paginationFilter);
    }

    public async Task<bool> AdvertisementExists(string id) =>
        await FindByCondition(a => a.Id == id, trackChanges: false)
            .AnyAsync();

    public void CreateAdvertisement(Advertisement advertisement) =>
        Create(advertisement);

    public void UpdateAdvertisement(Advertisement advertisement) =>
        Update(advertisement);

    public void DeleteAdvertisement(Advertisement advertisement) =>
        Delete(advertisement);
}

*/