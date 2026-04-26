using System.Security.Cryptography;
using System.Text;
using Kabutar.Service.Interfaces.Common;
using Microsoft.Extensions.Configuration;

namespace Kabutar.Service.Services.Common;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["ENCRYPTION_KEY"]
            ?? throw new InvalidOperationException("ENCRYPTION_KEY is not configured");

        // Derive a 32-byte key from the config value using SHA-256
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to cipher bytes, then Base64 encode
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var allBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        // Extract IV (first 16 bytes) and cipher bytes
        var iv = new byte[16];
        var cipherBytes = new byte[allBytes.Length - 16];
        Buffer.BlockCopy(allBytes, 0, iv, 0, 16);
        Buffer.BlockCopy(allBytes, 16, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
