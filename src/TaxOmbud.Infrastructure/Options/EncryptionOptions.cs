namespace TaxOmbud.Infrastructure.Options;

public class EncryptionOptions
{
    public const string SectionName = "Encryption";

    public string RsaPrivateKeyPem { get; set; } = string.Empty;
    public string RsaPublicKeyPem { get; set; } = string.Empty;
}
