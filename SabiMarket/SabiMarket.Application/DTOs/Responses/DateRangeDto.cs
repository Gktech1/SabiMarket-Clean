using SabiMarket.Application.DTOs.Requests;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs.Requests
{
    public class DateRangeDto
    {
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        // Optional properties for additional filtering
        public string DateRangeType { get; set; } // Daily, Weekly, Monthly, Yearly, Custom
        public string TimeZone { get; set; } // For handling different time zones

        // For preset date ranges
        public bool IsPreset { get; set; }
        public string PresetRange { get; set; } // Today, Yesterday, Last7Days, ThisMonth, LastMonth, ThisYear

        // Validation method to ensure StartDate is not after EndDate
        public bool IsValid()
        {
            return StartDate <= EndDate;
        }

        // Helper property to get the total number of days in the range
        public int DayCount => (EndDate - StartDate).Days + 1;
    }
}

