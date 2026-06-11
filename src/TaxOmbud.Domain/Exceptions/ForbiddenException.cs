using System;

namespace TaxOmbud.Domain.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException()
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }
}
