using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SabiMarket.Application.DTOs
{
    public class AssistantOfficerDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Level { get; set; }
        public string MarketId { get; set; }
    }

    public class CreateAssistantOfficerRequestDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public List<string> MarketIds { get; set; } = new List<string>();
        public string ProfileImage { get; set; }
    }

    public class UpdateAssistantOfficerRequestDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public List<string> MarketIds { get; set; } = new List<string>();
        public string ProfileImage { get; set; }
    }



 /*   public class UpdateAssistantOfficerRequestDto
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Gender { get; set; }

        public string? MarketId { get; set; }
    }*/

    public class BlockAssistantOfficerDto
    {
        public string AssistantOfficerId { get; set; }
        public bool IsBlocked { get; set; }
    }
}
