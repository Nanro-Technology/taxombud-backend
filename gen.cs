using System;
using System.Security.Cryptography;

class Program {
    static void Main() {
        using var rsa = RSA.Create(2048);
        string privateKey = rsa.ExportPkcs8PrivateKeyPem();
        string publicKey = rsa.ExportSubjectPublicKeyInfoPem();
        Console.WriteLine("PRIVATE:");
        Console.WriteLine(privateKey);
        Console.WriteLine("PUBLIC:");
        Console.WriteLine(publicKey);
    }
}
