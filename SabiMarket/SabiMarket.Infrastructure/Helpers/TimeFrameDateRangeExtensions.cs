using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.DTOs.Responses;
using SabiMarket.Domain.Enum;
using SabiMarket.Infrastructure.Helpers;

namespace SabiMarket.Application.Extensions
{
    public static class TimeFrameDateRangeExtensions
    {
        // Convert TimeFrame to the corresponding DateRangePresets constant
        public static string ToDateRangePreset(this TimeFrame timeFrame)
        {
            return timeFrame switch
            {
                TimeFrame.ThisWeek => DateRangePresets.Last7Days,
                TimeFrame.ThisMonth => DateRangePresets.ThisMonth,
                TimeFrame.ThisYear => DateRangePresets.ThisYear,
                TimeFrame.LastSixMonths => DateRangePresets.Custom, // Custom handling for 6 months
                TimeFrame.Custom => DateRangePresets.Custom,
                _ => DateRangePresets.ThisMonth // Default
            };
        }

        // Convert DateRangePresets constant to TimeFrame
        public static TimeFrame ToTimeFrame(this string dateRangePreset)
        {
            if (string.IsNullOrEmpty(dateRangePreset))
                return TimeFrame.ThisWeek; // Default value

            return dateRangePreset switch
            {
                DateRangePresets.Today => TimeFrame.Custom,
                DateRangePresets.Yesterday => TimeFrame.Custom,
                DateRangePresets.Last7Days => TimeFrame.ThisWeek,
                DateRangePresets.ThisMonth => TimeFrame.ThisMonth,
                DateRangePresets.LastMonth => TimeFrame.Custom,
                DateRangePresets.ThisYear => TimeFrame.ThisYear,
                DateRangePresets.Custom => TimeFrame.Custom,
                "This week" => TimeFrame.ThisWeek,
                "This month" => TimeFrame.ThisMonth,
                "This year" => TimeFrame.ThisYear,
                "Last 6 months" => TimeFrame.LastSixMonths,
                "last 6 months" => TimeFrame.LastSixMonths,
                "custom" => TimeFrame.Custom,
                _ => TimeFrame.ThisWeek // Default
            };
        }

        // Get DateRangeDto from TimeFrame
        public static DateRangeDto GetDateRange(this TimeFrame timeFrame)
        {
            // Special handling for LastSixMonths which isn't in DateRangePresets
            if (timeFrame == TimeFrame.LastSixMonths)
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                return new DateRangeDto
                {
                    StartDate = today.AddMonths(-6),
                    EndDate = today,
                    IsPreset = true,
                    PresetRange = "LastSixMonths", // Custom preset name
                    DateRangeType = "SemiAnnual"
                };
            }

            // For other timeframes, use the existing DateRangePresets
            var presetName = timeFrame.ToDateRangePreset();
            return DateRangePresets.GetPresetRange(presetName);
        }

        // Get TimeFrame display string
        public static string ToDisplayString(this TimeFrame timeFrame)
        {
            return timeFrame switch
            {
                TimeFrame.ThisWeek => "This week",
                TimeFrame.ThisMonth => "This month",
                TimeFrame.ThisYear => "This year",
                TimeFrame.LastSixMonths => "Last 6 months",
                TimeFrame.Custom => "Custom",
                _ => "This week"
            };
        }

        // Get all time frame options for dropdowns
        public static List<TimeFrameOption> GetTimeFrameOptions()
        {
            return new List<TimeFrameOption>
            {
                new TimeFrameOption { Value = TimeFrame.ThisWeek, Display = "This week" },
                new TimeFrameOption { Value = TimeFrame.ThisMonth, Display = "This month" },
                new TimeFrameOption { Value = TimeFrame.ThisYear, Display = "This year" },
                new TimeFrameOption { Value = TimeFrame.LastSixMonths, Display = "Last 6 months" },
                new TimeFrameOption { Value = TimeFrame.Custom, Display = "Custom" }
            };
        }
    }
}