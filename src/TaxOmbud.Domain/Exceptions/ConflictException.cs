using System;

namespace TaxOmbud.Domain.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException()
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }
}
