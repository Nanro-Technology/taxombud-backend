namespace TaxOmbud.Application.Interfaces.InfrastructureService;

public interface IEncryptionService
{
    /// <summary>Gets the server's public RSA key in PEM format.</summary>
    string GetPublicKeyPem();

    /// <summary>Decrypts the AES session key using the server's private RSA key.</summary>
    byte[] DecryptRsa(byte[] cipherText);

    /// <summary>Encrypts plaintext using the specified AES session key and IV.</summary>
    byte[] EncryptAesGcm(byte[] plaintext, byte[] key, byte[] iv, out byte[] tag);

    /// <summary>Decrypts ciphertext using the specified AES session key, IV, and authentication tag.</summary>
    byte[] DecryptAesGcm(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag);
}
