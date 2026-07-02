using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TaxOmbud.Infrastructure.Options;
using TaxOmbud.Application.Interfaces.InfrastructureService;

namespace TaxOmbud.Infrastructure.Services;

public class CryptoService : TaxOmbud.Application.Interfaces.InfrastructureService.ICryptoService, TaxOmbud.Application.Common.Interfaces.ICryptoService
{
    private readonly EncryptionOptions _options;

    public CryptoService(IOptions<EncryptionOptions> options)
    {
        _options = options.Value;
    }

    public string GetPublicKeyPem()
    {
        return _options.RsaPublicKeyPem;
    }

    public byte[] DecryptRsa(byte[] cipherText)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(_options.RsaPrivateKeyPem.ToCharArray());
        // Use OAEPSHA256 as it is the modern standard for RSA encryption
        return rsa.Decrypt(cipherText, RSAEncryptionPadding.OaepSHA256);
    }

    public byte[] EncryptAes(byte[] plainText, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(plainText, 0, plainText.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }

    public byte[] DecryptAes(byte[] cipherText, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
        cs.Write(cipherText, 0, cipherText.Length);
        cs.FlushFinalBlock();
        return ms.ToArray();
    }
}
