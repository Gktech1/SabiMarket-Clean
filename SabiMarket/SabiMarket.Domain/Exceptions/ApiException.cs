using Microsoft.AspNetCore.Http;

namespace SabiMarket.Domain.Exceptions
{
    public abstract class ApiException : Exception
    {
        public virtual int StatusCode { get; }
        protected ApiException(string message) : base(message) { }
    }

    public class NotFoundException : ApiException
    {
        public override int StatusCode => StatusCodes.Status404NotFound;
        public NotFoundException(string message) : base(message) { }
    }

    public class BadRequestException : ApiException
    {
        public override int StatusCode => StatusCodes.Status400BadRequest;
        public BadRequestException(string message) : base(message) { }
    }

    public class ForbidException : ApiException
    {
        public override int StatusCode => StatusCodes.Status403Forbidden;
        public ForbidException(string message) : base(message) { }
    }

    public class UnauthorizedException : ApiException
    {
        public override int StatusCode => StatusCodes.Status401Unauthorized;
        public UnauthorizedException(string message) : base(message) { }
    }

    public class ValidationException : ApiException
    {
        public override int StatusCode => StatusCodes.Status400BadRequest;
        public IEnumerable<string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new[] { message };
        }

        public ValidationException(IEnumerable<string> errors) : base("One or more validation errors occurred")
        {
            Errors = errors;
        }
    }
}