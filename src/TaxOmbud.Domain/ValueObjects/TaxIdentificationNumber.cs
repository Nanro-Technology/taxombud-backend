using System;
using System.Text.RegularExpressions;

namespace TaxOmbud.Domain.ValueObjects;

public record TaxIdentificationNumber
{
    private static readonly Regex TinRegex = new(@"^\d{10,12}$", RegexOptions.Compiled);

    public string Value { get; }

    public TaxIdentificationNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tax Identification Number cannot be empty.", nameof(value));
        }

        string cleaned = value.Replace("-", "").Replace(" ", "").Trim();

        if (!TinRegex.IsMatch(cleaned))
        {
            throw new ArgumentException("Tax Identification Number must be between 10 and 12 digits.", nameof(value));
        }

        Value = cleaned;
    }

    public override string ToString() => Value;
}
