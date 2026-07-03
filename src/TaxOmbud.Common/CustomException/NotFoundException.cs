using System.Net;

namespace TaxOmbud.Common.CustomException;

public class NotFoundException : ApplicationException
{
    public NotFoundException(string message = "The requested resource was not found.")
        : base(message, HttpStatusCode.NotFound) { }

    public NotFoundException(string resource, object key)
        : base($"{resource} with identifier '{key}' was not found.", HttpStatusCode.NotFound) { }
}
