using System.Net;

namespace TaxOmbud.Common.CustomException;

public class DomainException : ApplicationException
{
    public DomainException(string message)
        : base(message, HttpStatusCode.BadRequest) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
