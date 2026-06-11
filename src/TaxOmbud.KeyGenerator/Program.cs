using System;
using System.IO;
using System.Security.Cryptography;

namespace TaxOmbud.KeyGenerator;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Generating 2048-bit RSA key pair...");
            using var rsa = RSA.Create(2048);

            string privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
            string publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();

            string currentDir = Directory.GetCurrentDirectory();
            string privateKeyPath = Path.Combine(currentDir, "private.pem");
            string publicKeyPath = Path.Combine(currentDir, "public.pem");

            File.WriteAllText(privateKeyPath, privateKeyPem);
            File.WriteAllText(publicKeyPath, publicKeyPem);

            Console.WriteLine("RSA key pair generated successfully!");
            Console.WriteLine($"Private key: {privateKeyPath}");
            Console.WriteLine($"Public key: {publicKeyPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error generating keys: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
