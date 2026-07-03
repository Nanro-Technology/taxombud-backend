using System.Text.RegularExpressions;

namespace TaxOmbud.Common.Utilities;

public static class Helper
{
    /// <summary>Converts a display name to a URL-safe slug. E.g. "Tax Ombud" → "tax-ombud".</summary>
    public static string ToSlug(string value)
    {
        var slug = value.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug;
    }

    /// <summary>Masks an email address: "john.doe@example.com" → "jo***@example.com".</summary>
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return email;
        var parts = email.Split('@');
        if (parts.Length != 2) return email;
        var local = parts[0];
        var masked = local.Length <= 2
            ? local
            : local[..2] + new string('*', local.Length - 2);
        return $"{masked}@{parts[1]}";
    }

    /// <summary>Masks a phone number, showing only the last 4 digits: "0812345678" → "******5678".</summary>
    public static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4) return phone;
        return new string('*', phone.Length - 4) + phone[^4..];
    }

    /// <summary>Generates a random numeric OTP of the specified length.</summary>
    public static string GenerateNumericOtp(int length = 6)
    {
        var random = new Random();
        return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10).ToString()));
    }

    /// <summary>Returns the current UTC timestamp formatted for audit logs.</summary>
    public static string UtcNowFormatted() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");

    /// <summary>Safely parses a string to a Guid; returns Guid.Empty on failure.</summary>
    public static Guid ParseGuidSafe(string? value)
        => Guid.TryParse(value, out var result) ? result : Guid.Empty;

    /// <summary>Clamps a page size value between 1 and the provided maximum.</summary>
    public static int ClampPageSize(int pageSize, int max = 100)
        => Math.Clamp(pageSize, 1, max);
}

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
