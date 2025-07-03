using SabiMarket.Application.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Infrastructure.Helpers
{
    // Optional helper class for preset date ranges
    public static class DateRangePresets
    {
        public const string Today = "Today";
        public const string Yesterday = "Yesterday";
        public const string Last7Days = "Last7Days";
        public const string ThisMonth = "ThisMonth";
        public const string LastMonth = "LastMonth";
        public const string ThisYear = "ThisYear";
        public const string Custom = "Custom";

        public static DateRangeDto GetPresetRange(string preset)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            return preset switch
            {
                Today => new DateRangeDto
                {
                    StartDate = today,
                    EndDate = today,
                    IsPreset = true,
                    PresetRange = Today,
                    DateRangeType = "Daily"
                },

                Yesterday => new DateRangeDto
                {
                    StartDate = today.AddDays(-1),
                    EndDate = today.AddDays(-1),
                    IsPreset = true,
                    PresetRange = Yesterday,
                    DateRangeType = "Daily"
                },

                Last7Days => new DateRangeDto
                {
                    StartDate = today.AddDays(-7),
                    EndDate = today,
                    IsPreset = true,
                    PresetRange = Last7Days,
                    DateRangeType = "Weekly"
                },

                ThisMonth => new DateRangeDto
                {
                    StartDate = new DateTime(now.Year, now.Month, 1),
                    EndDate = today,
                    IsPreset = true,
                    PresetRange = ThisMonth,
                    DateRangeType = "Monthly"
                },

                LastMonth => new DateRangeDto
                {
                    StartDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1),
                    EndDate = new DateTime(now.Year, now.Month, 1).AddDays(-1),
                    IsPreset = true,
                    PresetRange = LastMonth,
                    DateRangeType = "Monthly"
                },

                ThisYear => new DateRangeDto
                {
                    StartDate = new DateTime(now.Year, 1, 1),
                    EndDate = today,
                    IsPreset = true,
                    PresetRange = ThisYear,
                    DateRangeType = "Yearly"
                },

                _ => new DateRangeDto
                {
                    StartDate = today,
                    EndDate = today,
                    IsPreset = false,
                    PresetRange = Custom,
                    DateRangeType = "Custom"
                }
            };
        }
    }
}
