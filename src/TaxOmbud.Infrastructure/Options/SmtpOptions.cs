namespace TaxOmbud.Infrastructure.Options;

public class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string Host { get; init; } = null!;
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string FromAddress { get; init; } = null!;
    public string FromName { get; init; } = "Tax Ombud System";
}

public class ConnectionStringOptions
{
    public const string SectionName = "ConnectionStrings";
    public string DefaultConnection { get; init; } = null!;
    public string Redis { get; init; } = null!;
}
