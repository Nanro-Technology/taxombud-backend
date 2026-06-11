namespace TaxOmbud.Application.Common.Interfaces;

public interface ICryptoService
{
    string GetPublicKeyPem();
    byte[] DecryptRsa(byte[] cipherText);
    byte[] EncryptAes(byte[] plainText, byte[] key, byte[] iv);
    byte[] DecryptAes(byte[] cipherText, byte[] key, byte[] iv);
}
