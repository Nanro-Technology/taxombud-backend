using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using TaxOmbud.Application.Common.Interfaces;

namespace TaxOmbud.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly string _privateKeyPem;
    private readonly string _publicKeyPem;

    public EncryptionService(IConfiguration configuration)
    {
        _privateKeyPem = configuration["Encryption:RsaPrivateKeyPem"] 
            ?? throw new ArgumentException("RsaPrivateKeyPem is missing from configuration");
        _publicKeyPem = configuration["Encryption:RsaPublicKeyPem"]
            ?? throw new ArgumentException("RsaPublicKeyPem is missing from configuration");
    }

    public string GetPublicKeyPem() => _publicKeyPem;

    public byte[] DecryptRsa(byte[] cipherText)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(_privateKeyPem.ToCharArray());
        // Using RSAEncryptionPadding.OaepSHA256 for Bank-Grade security
        return rsa.Decrypt(cipherText, RSAEncryptionPadding.OaepSHA256);
    }

    public byte[] EncryptAesGcm(byte[] plaintext, byte[] key, byte[] iv, out byte[] tag)
    {
        tag = new byte[16]; // 128-bit authentication tag
        var ciphertext = new byte[plaintext.Length];

        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(iv, plaintext, ciphertext, tag);

        return ciphertext;
    }

    public byte[] DecryptAesGcm(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag)
    {
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, tag.Length);
        aes.Decrypt(iv, ciphertext, tag, plaintext);

        return plaintext;
    }
}
