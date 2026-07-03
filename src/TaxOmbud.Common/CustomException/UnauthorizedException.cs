using System.Net;

namespace TaxOmbud.Common.CustomException;

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message = "Authentication is required to access this resource.")
        : base(message, HttpStatusCode.Unauthorized) { }
}
