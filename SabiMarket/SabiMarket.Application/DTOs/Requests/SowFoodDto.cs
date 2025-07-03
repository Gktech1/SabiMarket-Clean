using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    public class CreateSowFoodStaffDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public string ImageUrl { get; set; }
        public string SowFoodCompanyId { get; set; }
    }
    public class UpdateSowFoodStaffDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public string ImageUrl { get; set; }
    }
}
