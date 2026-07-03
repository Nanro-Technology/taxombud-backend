using System.Net;

namespace TaxOmbud.Common.CustomException;

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, HttpStatusCode.Forbidden) { }
}
