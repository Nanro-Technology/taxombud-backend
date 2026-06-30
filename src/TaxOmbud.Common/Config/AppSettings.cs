namespace TaxOmbud.Common.Config;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public int OtpExpiryMinutes { get; init; } = 10;
    public int DefaultPageSize { get; init; } = 20;
    public int MaxPageSize { get; init; } = 100;
    public string FrontendBaseUrl { get; init; } = string.Empty;
    public string StorageBasePath { get; init; } = "uploads";
    public bool EnableAuditLogging { get; init; } = true;
}
