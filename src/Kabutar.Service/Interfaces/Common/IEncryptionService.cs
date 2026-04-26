namespace Kabutar.Service.Interfaces.Common;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
