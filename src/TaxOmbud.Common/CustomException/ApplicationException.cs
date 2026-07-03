using System.Net;

namespace TaxOmbud.Common.CustomException;

public class ApplicationException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApplicationException() { }

    public ApplicationException(string message)
        : base(message) { }

    public ApplicationException(string message, Exception inner)
        : base(message, inner) { }

    public ApplicationException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
