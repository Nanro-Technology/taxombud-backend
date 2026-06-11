using System;

namespace TaxOmbud.Domain.ValueObjects;

public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(value));
        }

        string trimmed = value.Trim();
        if (!trimmed.Contains("@") || !trimmed.Contains("."))
        {
            throw new ArgumentException("Invalid email format.", nameof(value));
        }

        Value = trimmed.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
