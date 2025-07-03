namespace SabiMarket.Application.DTOs.Responses
{
    // TeamMemberResponseDto.cs
    public class TeamMemberResponseDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime DateAdded { get; set; }
        public string DefaultPassword { get; set; }
    }
}
