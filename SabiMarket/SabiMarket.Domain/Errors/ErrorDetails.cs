namespace SabiMarket.Domain.Models
{
    public class ErrorDetails
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
