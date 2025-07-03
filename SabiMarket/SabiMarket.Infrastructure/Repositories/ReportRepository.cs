using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Extensions;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SabiMarket.Infrastructure.Repositories
{
    public class ReportRepository : GeneralRepository<Report>, IReportRepository
    {
        private readonly ApplicationDbContext _context;
        public ReportRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Report> GetDashboardSummary()
        {
            var markets = await _context.Markets.CountAsync();
            var totalRevenue = await _context.LevyPayments.SumAsync(x => x.Amount);
            return new Report
            {
                MarketCount = markets,
                TotalRevenueGenerated = totalRevenue,
                ReportDate = DateTime.UtcNow
            };
        }

        public async Task<Report> GetDailyMetricsAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var dailyRevenue = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .SumAsync(lp => lp.Amount);

            var dailyTransactions = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .CountAsync();

            var activeMarkets = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .Select(lp => lp.MarketId)
                .Distinct()
                .CountAsync();

            var newTraders = await _context.Traders
                .Where(t => t.CreatedAt >= startOfDay && t.CreatedAt <= endOfDay)
                .CountAsync();

            return new Report
            {
                ReportDate = date,
                TotalRevenueGenerated = dailyRevenue,
                PaymentTransactions = dailyTransactions,
                ActiveMarkets = activeMarkets,
                NewTradersCount = newTraders,
                IsDaily = true
            };
        }

        public async Task<Report> GetMetricsAsync(DateTime startDate, DateTime endDate)
        {
            // Get total traders and caretakers
            var totalTraders = await _context.Traders.CountAsync();
            var totalCaretakers = await _context.Caretakers.CountAsync();

            // Get active markets - modified to check for non-null CaretakerId or having traders
            var activeMarkets = await _context.Markets
                .Where(m => m.Traders.Any() || m.CaretakerId != null)
                .CountAsync();

            // Get payment transactions and revenue
            var levyPayments = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .ToListAsync();
            var paymentTransactions = levyPayments.Count;
            var totalRevenue = levyPayments.Sum(lp => lp.Amount);

            // Calculate compliance rate
            var tradersWithPayments = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .Select(lp => lp.TraderId)
                .Distinct()
                .CountAsync();

            var complianceRate = totalTraders > 0
                ? (decimal)tradersWithPayments / totalTraders * 100
                : 0;

            return new Report
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalTraders = totalTraders,
                TotalCaretakers = totalCaretakers,
                TotalRevenueGenerated = totalRevenue,
                PaymentTransactions = paymentTransactions,
                ActiveMarkets = activeMarkets,
                ComplianceRate = complianceRate
            };
        }

        public async Task<Report> ExportAdminReport(
    DateTime startDate,
    DateTime endDate,
    string marketId = null,
    string lgaId = null,
    string timeZone = "UTC")
        {
            // Start with base query for levy payments within date range
            var paymentsQuery = _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate);

            // Get all payments matching our criteria
            var allPayments = await paymentsQuery
                .Select(lp => new
                {
                    lp.Id,
                    lp.MarketId,
                    MarketName = lp.Market.MarketName,
                    lp.Amount,
                    lp.PaymentMethod,
                    PaymentDate = lp.PaymentDate,
                    Year = lp.PaymentDate.Year,
                    Month = lp.PaymentDate.Month
                })
                .ToListAsync();

            // Filter payments if market is specified
            if (!string.IsNullOrEmpty(marketId))
            {
                allPayments = allPayments.Where(p => p.MarketId == marketId).ToList();
            }

            // Get market data separately
            var marketQuery = _context.Markets.AsQueryable();
            if (!string.IsNullOrEmpty(marketId))
            {
                marketQuery = marketQuery.Where(m => m.Id == marketId);
            }
            if (!string.IsNullOrEmpty(lgaId) && lgaId != "string")
            {
                marketQuery = marketQuery.Where(m => m.LocalGovernmentId == lgaId);
            }

            // Load market data
            var markets = await marketQuery
                .Select(m => new
                {
                    MarketId = m.Id,
                    MarketName = m.MarketName,
                    Location = m.Location,
                    TotalTraders = m.TotalTraders,
                    CompliantTraders = m.CompliantTraders,
                    ComplianceRate = m.ComplianceRate
                })
                .ToListAsync();

            // Get total revenue and transaction count
            var totalRevenue = allPayments.Sum(p => p.Amount);
            var totalTransactions = allPayments.Count;

            // Get trader statistics
            var totalTraders = markets.Sum(m => m.TotalTraders);
            var compliantTraders = markets.Sum(m => m.CompliantTraders);
            var averageComplianceRate = totalTraders > 0
                ? (decimal)compliantTraders / totalTraders * 100
                : 0;

            // Calculate average daily revenue
            var dayCount = Math.Max(1, (endDate - startDate).Days + 1);
            var dailyAverage = totalRevenue / dayCount;

            // Group payments by market to get market details
            var marketDetails = markets.Select(market => {
                var marketPayments = allPayments.Where(p => p.MarketId == market.MarketId).ToList();
                return new MarketDetail
                {
                    MarketId = market.MarketId,
                    MarketName = market.MarketName,
                    Location = market.Location,
                    TotalTraders = market.TotalTraders,
                    Revenue = marketPayments.Sum(p => p.Amount),
                    ComplianceRate = market.ComplianceRate,
                    TransactionCount = marketPayments.Count
                };
            }).ToList();

            // Group payments by month and market to get monthly data
            var monthlyGroups = allPayments
                .GroupBy(p => new { p.MarketId, p.MarketName, p.Year, p.Month })
                .Select(g => new
                {
                    g.Key.MarketId,
                    g.Key.MarketName,
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(p => p.Amount),
                    Transactions = g.Count()
                })
                .ToList();

            // Group by payment method
            var paymentMethodGroups = allPayments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Amount = g.Sum(p => p.Amount) })
                .ToList();

            // Convert payment methods to dictionary
            var paymentMethodDictionary = new Dictionary<string, decimal>();
            foreach (var pm in paymentMethodGroups)
            {
                string methodName;
                if (Enum.IsDefined(typeof(PaymenPeriodEnum), pm.Method))
                {
                    methodName = Enum.GetName(typeof(PaymenPeriodEnum), pm.Method);
                }
                else
                {
                    methodName = "Unknown";
                }

                paymentMethodDictionary[methodName] = pm.Amount;
            }

            // Build the report
            var report = new Report
            {
                MarketCount = markets.Count,
                TotalRevenueGenerated = totalRevenue,
                ReportDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                PaymentTransactions = totalTransactions,
                TotalTraders = totalTraders,
                CompliantTraders = compliantTraders,
                ComplianceRate = averageComplianceRate,
                DailyAverageRevenue = dailyAverage,
                RevenueByPaymentMethod = paymentMethodDictionary,
                MarketDetails = marketDetails,
                MonthlyRevenueData = monthlyGroups.Select(md => new MonthlyRevenue
                {
                    MarketId = md.MarketId,
                    MarketName = md.MarketName,
                    Year = md.Year,
                    Month = md.Month,
                    Revenue = md.Revenue,
                    TransactionCount = md.Transactions
                }).ToList()
            };

            // Handle timezone conversion
            if (timeZone != "UTC" && timeZone != "string")
            {
                try
                {
                    var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    report.ReportDate = TimeZoneInfo.ConvertTimeFromUtc(report.ReportDate, timezone);
                }
                catch (Exception ex)
                {
                    // Handle timezone exception
                    System.Diagnostics.Debug.WriteLine($"Error converting timezone: {ex.Message}");
                }
            }

            // Debug logging to see the values
            Console.WriteLine($"Report Summary: Markets: {report.MarketCount}, Revenue: {report.TotalRevenueGenerated}, Transactions: {report.PaymentTransactions}");
            foreach (var market in report.MarketDetails)
            {
                Console.WriteLine($"Market: {market.MarketName}, Revenue: {market.Revenue}");
            }

            return report;
        }

        /*     public async Task<Report> ExportAdminReport(
         DateTime startDate,
         DateTime endDate,
         string marketId = null,
         string lgaId = null,
         string timeZone = "UTC")
             {
                 // Start with base query for levy payments within date range
                 var query = _context.LevyPayments
                     .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate);

                 // Add market filter if specified
                 if (!string.IsNullOrEmpty(marketId))
                 {
                     query = query.Where(lp => lp.MarketId == marketId);
                 }

                 // Add LGA filter if specified
                 if (!string.IsNullOrEmpty(lgaId) && lgaId != "string")
                 {
                     query = query.Where(lp => lp.Market.LocalGovernmentId == lgaId);
                 }

                 // Get filtered market query
                 var marketQuery = _context.Markets.AsQueryable();
                 if (!string.IsNullOrEmpty(marketId))
                 {
                     marketQuery = marketQuery.Where(m => m.Id == marketId);
                 }
                 if (!string.IsNullOrEmpty(lgaId) && lgaId != "string")
                 {
                     marketQuery = marketQuery.Where(m => m.LocalGovernmentId == lgaId);
                 }

                 // Fetch complete market details
                 var marketDetails = await marketQuery
                     .Select(m => new
                     {
                         MarketId = m.Id,
                         MarketName = m.MarketName, // Using MarketName from your entity
                         Location = m.Location,
                         TotalTraders = m.TotalTraders, // Using the properties that already exist in your entity
                         CompliantTraders = m.CompliantTraders,
                         ComplianceRate = m.ComplianceRate,
                         Revenue = query.Where(lp => lp.MarketId == m.Id).Sum(lp => lp.Amount), // Calculate revenue for date range
                         Transactions = query.Count(lp => lp.MarketId == m.Id)
                     })
                     .ToListAsync();

                 // Get total revenue from all matching payments
                 var totalRevenue = await query.SumAsync(lp => lp.Amount);

                 // Get total transaction count
                 var totalTransactions = await query.CountAsync();

                 // Get trader statistics from Markets
                 var totalTraders = marketDetails.Sum(m => m.TotalTraders);
                 var compliantTraders = marketDetails.Sum(m => m.CompliantTraders);
                 var averageComplianceRate = totalTraders > 0
                     ? (decimal)compliantTraders / totalTraders * 100
                     : 0;

                 // Calculate average daily revenue
                 var dayCount = Math.Max(1, (endDate - startDate).Days + 1);
                 var dailyAverage = totalRevenue / dayCount;

                 // Get monthly revenue data for charts
                 var monthlyData = await query
                     .GroupBy(lp => new {
                         lp.MarketId,
                         MarketName = lp.Market.MarketName,
                         Year = lp.PaymentDate.Year,
                         Month = lp.PaymentDate.Month
                     })
                     .Select(g => new {
                         g.Key.MarketId,
                         g.Key.MarketName,
                         g.Key.Year,
                         g.Key.Month,
                         Revenue = g.Sum(lp => lp.Amount),
                         Transactions = g.Count()
                     })
                     .ToListAsync();

                 // Build the report
                 var report = new Report
                 {
                     MarketCount = marketDetails.Count,
                     TotalRevenueGenerated = totalRevenue,
                     ReportDate = DateTime.UtcNow,
                     StartDate = startDate,
                     EndDate = endDate,
                     PaymentTransactions = totalTransactions,
                     TotalTraders = totalTraders,
                     CompliantTraders = compliantTraders,
                     ComplianceRate = averageComplianceRate,
                     DailyAverageRevenue = dailyAverage
                 };

                 // Get payment methods breakdown
                 var paymentMethods = await query
                     .GroupBy(lp => lp.PaymentMethod)
                     .Select(g => new { Method = g.Key, Amount = g.Sum(lp => lp.Amount) })
                     .ToListAsync();

                 // Convert enum values to readable strings
                 var paymentMethodDictionary = new Dictionary<string, decimal>();
                 foreach (var pm in paymentMethods)
                 {
                     string methodName;
                     if (Enum.IsDefined(typeof(PaymentMethodEnum), pm.Method))
                     {
                         methodName = Enum.GetName(typeof(PaymentMethodEnum), pm.Method);
                     }
                     else
                     {
                         methodName = "Unknown";
                     }

                     paymentMethodDictionary[methodName] = pm.Amount;
                 }

                 // Assign to report
                 report.RevenueByPaymentMethod = paymentMethodDictionary;

                 // Add market details and monthly revenue collections to the report
                 // (You'll need to make sure these properties exist on your Report class)
                 report.MarketDetails = marketDetails.Select(m => new MarketDetail
                 {
                     MarketId = m.MarketId,
                     MarketName = m.MarketName,
                     Location = m.Location,
                     TotalTraders = m.TotalTraders,
                     Revenue = m.Revenue,
                     ComplianceRate = m.ComplianceRate,
                     TransactionCount = m.Transactions
                 }).ToList();

                 report.MonthlyRevenueData = monthlyData.Select(md => new MonthlyRevenue
                 {
                     MarketId = md.MarketId,
                     MarketName = md.MarketName,
                     Year = md.Year,
                     Month = md.Month,
                     Revenue = md.Revenue,
                     TransactionCount = md.Transactions
                 }).ToList();

                 // Handle timezone conversion
                 if (timeZone != "UTC" && timeZone != "string")
                 {
                     try
                     {
                         var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                         report.ReportDate = TimeZoneInfo.ConvertTimeFromUtc(report.ReportDate, timezone);
                     }
                     catch (Exception ex)
                     {
                         // Handle timezone exception - just keep UTC
                         System.Diagnostics.Debug.WriteLine($"Error converting timezone: {ex.Message}");
                     }
                 }

                 return report;
             }
     */
        public async Task<DashboardReportDto> GetDashboardReportDataAsync(
           string lgaFilter = null,
           string marketFilter = null,
           int? year = null,
           TimeFrame timeFrame = TimeFrame.ThisWeek)
        {
            // Get date range from timeframe or year
            DateRangeDto dateRange;

            if (year.HasValue)
            {
                // If year is specified, create a date range for the entire year
                dateRange = new DateRangeDto
                {
                    StartDate = new DateTime(year.Value, 1, 1),
                    EndDate = new DateTime(year.Value, 12, 31),
                    IsPreset = true,
                    PresetRange = "YearlyCustom",
                    DateRangeType = "Yearly"
                };
            }
            else
            {
                // Get DateRangeDto from TimeFrame
                dateRange = timeFrame.GetDateRange();
            }

            // Call the implementation using the date range
            return await GetDashboardDataByDateRangeAsync(
                lgaFilter,
                marketFilter,
                year,
                timeFrame,
                dateRange);
        }

        public async Task<DashboardReportDto> GetDashboardDataByDateRangeAsync(
            string lgaFilter = null,
            string marketFilter = null,
            int? year = null,
            TimeFrame timeFrame = TimeFrame.Custom,
            DateRangeDto dateRange = null)
        {
            // Use provided date range or default to current month
            if (dateRange == null)
            {
                dateRange = TimeFrame.ThisMonth.GetDateRange();
            }

            var startDate = dateRange.StartDate;
            var endDate = dateRange.EndDate;

            // Apply filters to market query
            var marketsQuery = _context.Markets.AsQueryable();
            if (!string.IsNullOrEmpty(lgaFilter))
            {
                marketsQuery = marketsQuery.Where(m => m.LocalGovernmentName == lgaFilter);
            }
            if (!string.IsNullOrEmpty(marketFilter))
            {
                marketsQuery = marketsQuery.Where(m => m.MarketName == marketFilter);
            }

            // Get markets
            var markets = await marketsQuery.ToListAsync();
            var marketIds = markets.Select(m => m.Id).ToList();

            // 1. Market Count Card Data
            var marketCount = new MarketCountDto
            {
                Count = markets.Count,
                Description = "Total Number of registered markets"
            };

            // 2. Total Revenue Card Data
            var totalRevenue = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate &&
                           lp.PaymentDate <= endDate &&
                           marketIds.Contains(lp.MarketId))
                .SumAsync(lp => lp.Amount);

            var totalRevenueDto = new TotalRevenueDto
            {
                Amount = totalRevenue,
                TimeFrame = timeFrame,
                TimeFrameDisplay = dateRange.IsPreset ? dateRange.PresetRange : timeFrame.ToDisplayString(),
                Description = "Total levy paid"
            };

            // 3. Levy Payments Breakdown Graph Data
            var monthlyData = await GetMonthlyLevyDataAsync(startDate, endDate, marketIds);

            // 4. Compliance Rates Donut Chart
            var complianceData = await GetComplianceRatesAsync(startDate, endDate, markets, year);

            // 5. Levy Collection Per Market
            var levyCollection = await GetLevyCollectionPerMarketAsync(startDate, endDate, markets, year);

            // Create and return the dashboard DTO
            return new DashboardReportDto
            {
                MarketCount = marketCount,
                TotalRevenue = totalRevenueDto,
                LevyPayments = monthlyData,
                ComplianceRates = complianceData,
                LevyCollection = levyCollection,
                CurrentDateTime = DateTime.Now
            };
        }

        private async Task<LevyPaymentsBreakdownDto> GetMonthlyLevyDataAsync(
            DateTime startDate,
            DateTime endDate,
            List<string> marketIds) // Changed from List<Guid> to List<string>
        {
            // Generate months dynamically based on the date range
            var months = new List<string>();
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);

            while (currentDate <= endDate)
            {
                months.Add(currentDate.ToString("MMM"));
                currentDate = currentDate.AddMonths(1);
            }

            // Get all markets for which we need data
            var markets = await _context.Markets
                .Where(m => marketIds.Contains(m.Id))
                .OrderByDescending(m => _context.LevyPayments
                    .Where(lp => lp.MarketId == m.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .Sum(lp => lp.Amount))
                .Take(3) // Take top 3 by revenue
                .ToListAsync();

            // Define colors
            var colors = new[] { "#FF6B8E", "#20C997", "#FFD700" };

            // Get payments data for each market
            var marketData = new List<MarketMonthlyDataDto>();

            for (int i = 0; i < markets.Count; i++)
            {
                var market = markets[i];
                var values = new List<decimal>();

                // Get data for each month
                foreach (var month in months)
                {
                    var monthNum = DateTime.ParseExact(month, "MMM", CultureInfo.InvariantCulture).Month;
                    var year = currentDate.Month > monthNum ? currentDate.Year : startDate.Year;

                    var monthStart = new DateTime(year, monthNum, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    var amount = await _context.LevyPayments
                        .Where(lp => lp.MarketId == market.Id &&
                                  lp.PaymentDate >= monthStart &&
                                  lp.PaymentDate <= monthEnd)
                        .SumAsync(lp => lp.Amount);

                    values.Add(amount);
                }

                marketData.Add(new MarketMonthlyDataDto
                {
                    MarketName = market.MarketName,
                    Color = colors[i % colors.Length],
                    Values = values
                });
            }

            return new LevyPaymentsBreakdownDto
            {
                Months = months,
                MarketData = marketData
            };
        }

        private async Task<ComplianceRatesDto> GetComplianceRatesAsync(
            DateTime startDate,
            DateTime endDate,
            List<Market> markets,
            int? year = null)
        {
            var marketCompliance = new List<MarketReportComplianceDto>();
            var colors = new[] { "#FF6B8E", "#20C997", "#FFD700" };

            // Get the top markets by number of traders
            var marketsWithTraderCount = new List<(Market Market, int TraderCount)>();

            foreach (var market in markets)
            {
                var traderCount = await _context.Traders
                    .CountAsync(t => t.MarketId == market.Id);

                marketsWithTraderCount.Add((market, traderCount));
            }

            // Take top 3 markets by trader count
            var topMarkets = marketsWithTraderCount
                .OrderByDescending(m => m.TraderCount)
                .Take(3)
                .ToList();

            for (int i = 0; i < topMarkets.Count; i++)
            {
                var (market, totalTraders) = topMarkets[i];

                var compliantTraders = await _context.LevyPayments
                    .Where(lp => lp.MarketId == market.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .Select(lp => lp.TraderId)
                    .Distinct()
                    .CountAsync();

                int compliancePercentage = totalTraders > 0
                    ? (int)Math.Round((double)compliantTraders / totalTraders * 100)
                    : 0;

                marketCompliance.Add(new MarketReportComplianceDto
                {
                    MarketName = market.MarketName,
                    CompliancePercentage = compliancePercentage,
                    Color = colors[i % colors.Length]
                });
            }

            return new ComplianceRatesDto
            {
                Year = year ?? DateTime.Now.Year,
                MarketCompliance = marketCompliance
            };
        }

        private async Task<LevyCollectionPerMarketDto> GetLevyCollectionPerMarketAsync(
            DateTime startDate,
            DateTime endDate,
            List<Market> markets,
            int? year = null)
        {
            var marketLevy = new List<MarketLevyDto>();
            decimal totalAmount = 0;

            // Get markets sorted by revenue
            var marketsWithRevenue = new List<(Market Market, decimal Revenue)>();

            foreach (var market in markets)
            {
                var revenue = await _context.LevyPayments
                    .Where(lp => lp.MarketId == market.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .SumAsync(lp => lp.Amount);

                marketsWithRevenue.Add((market, revenue));
                totalAmount += revenue;
            }

            // Take top markets by revenue
            var topMarkets = marketsWithRevenue
                .OrderByDescending(m => m.Revenue)
                .Take(3)
                .ToList();

            foreach (var (market, revenue) in topMarkets)
            {
                marketLevy.Add(new MarketLevyDto
                {
                    MarketName = market.MarketName,
                    Amount = revenue
                });
            }

            return new LevyCollectionPerMarketDto
            {
                Year = year ?? DateTime.Now.Year,
                TotalAmount = totalAmount,
                MarketLevy = marketLevy
            };
        }

        public async Task<FilterOptionsDto> GetFilterOptionsAsync()
        {
            // Get all LGAs - fixed to use LocalGovernmentName
            var lgas = await _context.Markets
                .Select(m => m.LocalGovernmentName)
                .Where(lga => !string.IsNullOrEmpty(lga))
                .Distinct()
                .ToListAsync();

            // Get all Markets
            var markets = await _context.Markets
                .Select(m => m.MarketName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToListAsync();

            // Get all years with data
            var years = await _context.LevyPayments
                .Select(lp => lp.PaymentDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // If no historical data, add current year
            if (!years.Any())
            {
                years.Add(DateTime.Now.Year);
            }

            return new FilterOptionsDto
            {
                LGAs = lgas,
                Markets = markets,
                Years = years,
                TimeFrames = TimeFrameDateRangeExtensions.GetTimeFrameOptions()
            };
        }

        public async Task<IEnumerable<Report>> GetLevyPaymentsBreakdown(int year)
        {
            var payments = await _context.LevyPayments
                .Where(x => x.PaymentDate.Year == year)
                .GroupBy(x => new { x.MarketId, x.Market.MarketName, Month = x.PaymentDate.Month })
                .Select(g => new Report
                {
                    MarketId = g.Key.MarketId,
                    MarketName = g.Key.MarketName,
                    MonthlyRevenue = g.Sum(x => x.Amount),
                    Month = g.Key.Month,
                    Year = year
                })
                .ToListAsync();
            return payments;
        }

        public async Task<Report> GetMarketComplianceRates(string marketId)
        {
            var market = await _context.Markets
                .Include(m => m.Traders)
                .FirstOrDefaultAsync(m => m.Id == marketId);
            if (market == null)
                return null;
            var tradersWithPayments = await _context.LevyPayments
                .Where(lp => lp.MarketId == marketId)
                .Select(lp => lp.TraderId)
                .Distinct()
                .CountAsync();
            return new Report
            {
                MarketId = marketId,
                MarketName = market.MarketName,
                TotalTraders = market.Traders?.Count ?? 0,
                CompliantTraders = tradersWithPayments,
                ComplianceRate = market.Traders?.Count > 0
                    ? (decimal)tradersWithPayments / market.Traders.Count * 100
                    : 0
            };
        }

        public async Task<IEnumerable<Report>> GetLevyCollectionPerMarket()
        {
            return await _context.Markets
                .Select(m => new Report
                {
                    MarketId = m.Id,
                    MarketName = m.MarketName,
                    TotalLevyCollected = m.Traders
                        .SelectMany(t => t.LevyPayments)
                        .Sum(lp => lp.Amount)
                })
                .ToListAsync();
        }

        // Update the repository method to accept all filter parameters
  /*      public async Task<Report> ExportAdminReport(
            DateTime startDate,
            DateTime endDate,
            string marketId = null,
            string lgaId = null,
            string timeZone = "UTC")
        {
            // Start with base query for levy payments within date range
            var query = _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate);

            // Add market filter if specified
            if (!string.IsNullOrEmpty(marketId))
            {
                query = query.Where(lp => lp.MarketId == marketId);
            }

            // Add LGA filter if specified
            if (!string.IsNullOrEmpty(lgaId))
            {
                query = query.Where(lp => lp.Market.LocalGovernmentId == lgaId);
            }

            // Get filtered market count - moved this up to avoid duplication
            var marketQuery = _context.Markets.AsQueryable();
            if (!string.IsNullOrEmpty(marketId))
            {
                marketQuery = marketQuery.Where(m => m.Id == marketId);
            }
            if (!string.IsNullOrEmpty(lgaId))
            {
                marketQuery = marketQuery.Where(m => m.LocalGovernmentId == lgaId);
            }

            // Build the report with filtered data - only create this once
            var report = new Report
            {
                MarketCount = await marketQuery.CountAsync(),
                TotalRevenueGenerated = await query.SumAsync(lp => lp.Amount),
                ReportDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate
            };

            // Get payment methods breakdown
            var paymentMethods = await query
                .GroupBy(lp => lp.PaymentMethod)
                .Select(g => new { Method = g.Key, Amount = g.Sum(lp => lp.Amount) })
                .ToListAsync();

            // Convert enum values to readable strings
            var paymentMethodDictionary = new Dictionary<string, decimal>();
            foreach (var pm in paymentMethods)
            {
                string methodName;
                if (Enum.IsDefined(typeof(PaymentMethodEnum), pm.Method))
                {
                    methodName = Enum.GetName(typeof(PaymentMethodEnum), pm.Method);
                }
                else
                {
                    methodName = "Unknown";
                }

                paymentMethodDictionary[methodName] = pm.Amount;
            }

            // Assign to report
            report.RevenueByPaymentMethod = paymentMethodDictionary;

            // Convert time if needed
            if (timeZone != "UTC")
            {
                try
                {
                    // Check if the timeZone is actually the literal "string" (probably from the default in your DTO)
                    if (timeZone == "string")
                    {
                        // Default to UTC if "string" is passed
                        timeZone = "UTC";
                    }
                    else
                    {
                        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                        report.ReportDate = TimeZoneInfo.ConvertTimeFromUtc(report.ReportDate, timezone);
                    }
                }
                catch (TimeZoneNotFoundException)
                {
                    // Log the invalid timezone
                    System.Diagnostics.Debug.WriteLine($"Invalid timezone: {timeZone}");
                    // Just continue using UTC
                }
                catch (Exception ex)
                {
                    // Handle other potential exceptions
                    System.Diagnostics.Debug.WriteLine($"Error converting timezone: {ex.Message}");
                }
            }

            return report;
        }*/

        public async Task<Report> ExportReport(DateTime startDate, DateTime endDate)
        {
            var report = new Report
            {
                MarketCount = await _context.Markets.CountAsync(),
                TotalRevenueGenerated = await _context.LevyPayments
                    .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                    .SumAsync(lp => lp.Amount),
                ReportDate = DateTime.UtcNow
            };
            return report;
        }
    }
}


/*using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Application.Extensions;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Data;
using System.Globalization;
using System.Linq;

namespace SabiMarket.Infrastructure.Repositories
{
    public class ReportRepository : GeneralRepository<Report>, IReportRepository
    {
        private readonly ApplicationDbContext _context;
        public ReportRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Report> GetDashboardSummary()
        {
            var markets = await _context.Markets.CountAsync();
            var totalRevenue = await _context.LevyPayments.SumAsync(x => x.Amount);
            return new Report
            {
                MarketCount = markets,
                TotalRevenueGenerated = totalRevenue,
                ReportDate = DateTime.UtcNow
            };
        }

        public async Task<Report> GetDailyMetricsAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var dailyRevenue = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .SumAsync(lp => lp.Amount);

            var dailyTransactions = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .CountAsync();

            var activeMarkets = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startOfDay && lp.PaymentDate <= endOfDay)
                .Select(lp => lp.MarketId)
                .Distinct()
                .CountAsync();

            var newTraders = await _context.Traders
                .Where(t => t.CreatedAt >= startOfDay && t.CreatedAt <= endOfDay)
                .CountAsync();

            return new Report
            {
                ReportDate = date,
                TotalRevenueGenerated = dailyRevenue,
                PaymentTransactions = dailyTransactions,
                ActiveMarkets = activeMarkets,
                NewTradersCount = newTraders,
                IsDaily = true
            };
        }

        public async Task<Report> GetMetricsAsync(DateTime startDate, DateTime endDate)
        {
            // Get total traders and caretakers
            var totalTraders = await _context.Traders.CountAsync();
            var totalCaretakers = await _context.Caretakers.CountAsync();

            // Get active markets - modified to check for non-null CaretakerId or having traders
            var activeMarkets = await _context.Markets
                .Where(m => m.Traders.Any() || m.CaretakerId != null)
                .CountAsync();

            // Get payment transactions and revenue
            var levyPayments = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .ToListAsync();
            var paymentTransactions = levyPayments.Count;
            var totalRevenue = levyPayments.Sum(lp => lp.Amount);

            // Calculate compliance rate
            var tradersWithPayments = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .Select(lp => lp.TraderId)
                .Distinct()
                .CountAsync();

            var complianceRate = totalTraders > 0
                ? (decimal)tradersWithPayments / totalTraders * 100
                : 0;

            return new Report
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalTraders = totalTraders,
                TotalCaretakers = totalCaretakers,
                TotalRevenueGenerated = totalRevenue,
                PaymentTransactions = paymentTransactions,
                ActiveMarkets = activeMarkets,
                ComplianceRate = complianceRate
            };
        }

        public async Task<DashboardReportDto> GetDashboardDataAsync(
           string lgaFilter = null,
           string marketFilter = null,
           int? year = null,
           TimeFrame timeFrame = TimeFrame.ThisWeek)
        {
            // Get date range from timeframe or year
            DateRangeDto dateRange;

            if (year.HasValue)
            {
                // If year is specified, create a date range for the entire year
                dateRange = new DateRangeDto
                {
                    StartDate = new DateTime(year.Value, 1, 1),
                    EndDate = new DateTime(year.Value, 12, 31),
                    IsPreset = true,
                    PresetRange = "YearlyCustom",
                    DateRangeType = "Yearly"
                };
            }
            else
            {
                // Get DateRangeDto from TimeFrame
                dateRange = timeFrame.GetDateRange();
            }

            // Call the implementation using the date range
            return await GetDashboardDataByDateRangeAsync(
                lgaFilter,
                marketFilter,
                year,
                timeFrame,
                dateRange);
        }

        public async Task<DashboardReportDto> GetDashboardDataByDateRangeAsync(
            string lgaFilter = null,
            string marketFilter = null,
            int? year = null,
            TimeFrame timeFrame = TimeFrame.Custom,
            DateRangeDto dateRange = null)
        {
            // Use provided date range or default to current month
            if (dateRange == null)
            {
                dateRange = TimeFrame.ThisMonth.GetDateRange();
            }

            var startDate = dateRange.StartDate;
            var endDate = dateRange.EndDate;

            // Apply filters to market query
            var marketsQuery = _context.Markets.AsQueryable();
            if (!string.IsNullOrEmpty(lgaFilter))
            {
                marketsQuery = marketsQuery.Where(m => m.LocalGovernmentName == lgaFilter);

            }
            if (!string.IsNullOrEmpty(marketFilter))
            {
                marketsQuery = marketsQuery.Where(m => m.MarketName == marketFilter);
            }

            // Get markets
            var markets = await marketsQuery.ToListAsync();
            var marketIds = markets.Select(m => m.Id).ToList();

            // 1. Market Count Card Data
            var marketCount = new MarketCountDto
            {
                Count = markets.Count,
                Description = "Total Number of registered markets"
            };

            // 2. Total Revenue Card Data
            var totalRevenue = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate &&
                           lp.PaymentDate <= endDate &&
                           marketIds.Contains(lp.MarketId))
                .SumAsync(lp => lp.Amount);

            var totalRevenueDto = new TotalRevenueDto
            {
                Amount = totalRevenue,
                TimeFrame = timeFrame,
                TimeFrameDisplay = dateRange.IsPreset ? dateRange.PresetRange : timeFrame.ToDisplayString(),
                Description = "Total levy paid"
            };


            // 3. Levy Payments Breakdown Graph Data
            var monthlyData = await GetMonthlyLevyDataAsync(startDate, endDate, marketIds);

            // 4. Compliance Rates Donut Chart
            var complianceData = await GetComplianceRatesAsync(startDate, endDate, markets, year);

            // 5. Levy Collection Per Market
            var levyCollection = await GetLevyCollectionPerMarketAsync(startDate, endDate, markets, year);

            // Create and return the dashboard DTO
            return new DashboardReportDto
            {
                MarketCount = marketCount,
                TotalRevenue = totalRevenueDto,
                LevyPayments = monthlyData,
                ComplianceRates = complianceData,
                LevyCollection = levyCollection,
                CurrentDateTime = DateTime.Now
            };
        }

        private async Task<LevyPaymentsBreakdownDto> GetMonthlyLevyDataAsync(
            DateTime startDate,
            DateTime endDate,
            List<Guid> marketIds)
        {
            // Generate months dynamically based on the date range
            var months = new List<string>();
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);

            while (currentDate <= endDate)
            {
                months.Add(currentDate.ToString("MMM"));
                currentDate = currentDate.AddMonths(1);
            }

            // Get all markets for which we need data
            var markets = await _context.Markets
                .Where(m => marketIds.Contains(m.Id))
                .OrderByDescending(m => _context.LevyPayments
                    .Where(lp => lp.MarketId == m.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .Sum(lp => lp.Amount))
                .Take(3) // Take top 3 by revenue
                .ToListAsync();

            // Define colors
            var colors = new[] { "#FF6B8E", "#20C997", "#FFD700" };

            // Get payments data for each market
            var marketData = new List<MarketMonthlyDataDto>();

            for (int i = 0; i < markets.Count; i++)
            {
                var market = markets[i];
                var values = new List<decimal>();

                // Get data for each month
                foreach (var month in months)
                {
                    var monthNum = DateTime.ParseExact(month, "MMM", CultureInfo.InvariantCulture).Month;
                    var year = currentDate.Month > monthNum ? currentDate.Year : startDate.Year;

                    var monthStart = new DateTime(year, monthNum, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    var amount = await _context.LevyPayments
                        .Where(lp => lp.MarketId == market.Id &&
                                  lp.PaymentDate >= monthStart &&
                                  lp.PaymentDate <= monthEnd)
                        .SumAsync(lp => lp.Amount);

                    values.Add(amount);
                }

                marketData.Add(new MarketMonthlyDataDto
                {
                    MarketName = market.MarketName,
                    Color = colors[i % colors.Length],
                    Values = values
                });
            }

            return new LevyPaymentsBreakdownDto
            {
                Months = months,
                MarketData = marketData
            };
        }

        private async Task<ComplianceRatesDto> GetComplianceRatesAsync(
            DateTime startDate,
            DateTime endDate,
            List<Market> markets,
            int? year = null)
        {
            var marketCompliance = new List<MarketReportComplianceDto>();
            var colors = new[] { "#FF6B8E", "#20C997", "#FFD700" };

            // Get the top markets by number of traders
            var marketsWithTraderCount = new List<(Market Market, int TraderCount)>();

            foreach (var market in markets)
            {
                var traderCount = await _context.Traders
                    .CountAsync(t => t.MarketId == market.Id);

                marketsWithTraderCount.Add((market, traderCount));
            }

            // Take top 3 markets by trader count
            var topMarkets = marketsWithTraderCount
                .OrderByDescending(m => m.TraderCount)
                .Take(3)
                .ToList();

            for (int i = 0; i < topMarkets.Count; i++)
            {
                var (market, totalTraders) = topMarkets[i];

                var compliantTraders = await _context.LevyPayments
                    .Where(lp => lp.MarketId == market.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .Select(lp => lp.TraderId)
                    .Distinct()
                    .CountAsync();

                int compliancePercentage = totalTraders > 0
                    ? (int)Math.Round((double)compliantTraders / totalTraders * 100)
                    : 0;

                marketCompliance.Add(new MarketReportComplianceDto
                {
                    MarketName = market.MarketName,
                    CompliancePercentage = compliancePercentage,
                    Color = colors[i % colors.Length]
                });
            }

            return new ComplianceRatesDto
            {
                Year = year ?? DateTime.Now.Year,
                MarketCompliance = marketCompliance
            };
        }

        private async Task<LevyCollectionPerMarketDto> GetLevyCollectionPerMarketAsync(
            DateTime startDate,
            DateTime endDate,
            List<Market> markets,
            int? year = null)
        {
            var marketLevy = new List<MarketLevyDto>();
            decimal totalAmount = 0;

            // Get markets sorted by revenue
            var marketsWithRevenue = new List<(Market Market, decimal Revenue)>();

            foreach (var market in markets)
            {
                var revenue = await _context.LevyPayments
                    .Where(lp => lp.MarketId == market.Id &&
                              lp.PaymentDate >= startDate &&
                              lp.PaymentDate <= endDate)
                    .SumAsync(lp => lp.Amount);

                marketsWithRevenue.Add((market, revenue));
                totalAmount += revenue;
            }

            // Take top markets by revenue
            var topMarkets = marketsWithRevenue
                .OrderByDescending(m => m.Revenue)
                .Take(3)
                .ToList();

            foreach (var (market, revenue) in topMarkets)
            {
                marketLevy.Add(new MarketLevyDto
                {
                    MarketName = market.MarketName,
                    Amount = revenue
                });
            }

            return new LevyCollectionPerMarketDto
            {
                Year = year ?? DateTime.Now.Year,
                TotalAmount = totalAmount,
                MarketLevy = marketLevy
            };
        }

        public async Task<FilterOptionsDto> GetFilterOptionsAsync()
        {
            // Get all LGAs
            var lgas = await _context.Markets
                .Select(m => m.LocalGovernment)
                .Where(lga => !string.IsNullOrEmpty(lga))
                .Distinct()
                .ToListAsync();

            // Get all Markets
            var markets = await _context.Markets
                .Select(m => m.MarketName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToListAsync();

            // Get all years with data
            var years = await _context.LevyPayments
                .Select(lp => lp.PaymentDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            // If no historical data, add current year
            if (!years.Any())
            {
                years.Add(DateTime.Now.Year);
            }

            return new FilterOptionsDto
            {
                LGAs = lgas,
                Markets = markets,
                Years = years,
                TimeFrames = TimeFrameDateRangeExtensions.GetTimeFrameOptions()
            };
        }
    }
    *//*    public async Task<Report> GetMetricsAsync(DateTime startDate, DateTime endDate)
        {
            // Get total traders and caretakers
            var totalTraders = await _context.Traders.CountAsync();
            var totalCaretakers = await _context.Caretakers.CountAsync();

            // Get active markets
            var activeMarkets = await _context.Markets
                .Where(m => m.Traders.Any() || m.Caretaker.Any())
                .CountAsync();

            // Get payment transactions and revenue
            var levyPayments = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .ToListAsync();

            var paymentTransactions = levyPayments.Count;
            var totalRevenue = levyPayments.Sum(lp => lp.Amount);

            // Calculate compliance rate
            var tradersWithPayments = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .Select(lp => lp.TraderId)
                .Distinct()
                .CountAsync();

            var complianceRate = totalTraders > 0
                ? (decimal)tradersWithPayments / totalTraders * 100
                : 0;

            return new Report
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalTraders = totalTraders,
                TotalCaretakers = totalCaretakers,
                TotalRevenueGenerated = totalRevenue,
                PaymentTransactions = paymentTransactions,
                ActiveMarkets = activeMarkets,
                ComplianceRate = complianceRate
            };
        }*//*
    public async Task<IEnumerable<Report>> GetLevyPaymentsBreakdown(int year)
    {
        var payments = await _context.LevyPayments
            .Where(x => x.PaymentDate.Year == year)
            .GroupBy(x => new { x.MarketId, x.Market.MarketName, Month = x.PaymentDate.Month })
            .Select(g => new Report
            {
                MarketId = g.Key.MarketId,
                MarketName = g.Key.MarketName,
                MonthlyRevenue = g.Sum(x => x.Amount),
                Month = g.Key.Month,
                Year = year
            })
            .ToListAsync();
        return payments;
    }

    public async Task<Report> GetMarketComplianceRates(string marketId)
    {
        var market = await _context.Markets
            .Include(m => m.Traders)
            .FirstOrDefaultAsync(m => m.Id == marketId);
        if (market == null)
            return null;
        var tradersWithPayments = await _context.LevyPayments
            .Where(lp => lp.MarketId == marketId)
            .Select(lp => lp.TraderId)
            .Distinct()
            .CountAsync();
        return new Report
        {
            MarketId = marketId,
            MarketName = market.MarketName,
            TotalTraders = market.Traders?.Count ?? 0,
            CompliantTraders = tradersWithPayments,
            ComplianceRate = market.Traders?.Count > 0
                ? (decimal)tradersWithPayments / market.Traders.Count * 100
                : 0
        };
    }

    public async Task<IEnumerable<Report>> GetLevyCollectionPerMarket()
    {
        return await _context.Markets
            .Select(m => new Report
            {
                MarketId = m.Id,
                MarketName = m.MarketName,
                TotalLevyCollected = m.Traders
                    .SelectMany(t => t.LevyPayments)
                    .Sum(lp => lp.Amount)
            })
            .ToListAsync();
    }

    public async Task<Report> ExportReport(DateTime startDate, DateTime endDate)
    {
        var report = new Report
        {
            MarketCount = await _context.Markets.CountAsync(),
            TotalRevenueGenerated = await _context.LevyPayments
                .Where(lp => lp.PaymentDate >= startDate && lp.PaymentDate <= endDate)
                .SumAsync(lp => lp.Amount),
            ReportDate = DateTime.UtcNow
        };
        return report;
    }
}*/