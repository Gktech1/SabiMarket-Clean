using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Responses
{
    public class LocalGovernmentResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public string LGA { get; set; }
        public decimal CurrentRevenue { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public string Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class LocalGovernmentWithUsersResponseDto 
    {
        public UserDto User { get; set; }
        public LocalGovernmentResponseDto LocalGovernment { get; set; }
    }

    public class UsersByLocalGovernmentResponseDto
    {
        public UserDto User { get; set; }
        public LocalGovernmentResponseDto LocalGovernment { get; set; }
    }
}
