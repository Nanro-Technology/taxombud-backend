using System.Net;

namespace TaxOmbud.Common.CustomException;

public class ConflictException : ApplicationException
{
    public ConflictException(string message)
        : base(message, HttpStatusCode.Conflict) { }
}
