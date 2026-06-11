using System;

namespace TaxOmbud.Domain.ValueObjects;

/// <summary>
/// Value object for system reference numbers.
/// Format: PREFIX-YYYYMMDD-RANDOM6  e.g.  CMP-20260608-A3F91B
/// </summary>
public record ReferenceNumber
{
    public string Value { get; }

    private ReferenceNumber(string value) => Value = value;

    /// <summary>Creates and validates a reference number from an existing string.</summary>
    public static ReferenceNumber From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Reference number cannot be empty.", nameof(value));

        return new ReferenceNumber(value.Trim().ToUpperInvariant());
    }

    /// <summary>Generates a new unique reference number with the given prefix.</summary>
    /// <param name="prefix">e.g. CMP, CASE, APL, APT</param>
    public static string Generate(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Prefix cannot be empty.", nameof(prefix));

        var datePart = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"{prefix.ToUpperInvariant()}-{datePart}-{randomPart}";
    }

    public override string ToString() => Value;
}
