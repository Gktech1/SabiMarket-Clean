namespace SabiMarket.Application.DTOs.Responses
{
    public class BaseResponse<T>
    {
        public T Data { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }   
        public ErrorResponse Error { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string Type { get; set; } = "Error";
        public string StackTrace { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
