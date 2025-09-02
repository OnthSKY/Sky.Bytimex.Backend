using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Enums;

namespace Sky.Template.Backend.Core.Encryption;
public interface IEncryptionService
{
    string Encrypt(string plainText, EncryptionKeyType keyType);
    public string Decrypt(string encryptedText, EncryptionKeyType keyType);
}


public class AesEncryptionService : IEncryptionService
{
    private readonly EncryptionConfig _config;

    public AesEncryptionService(IOptions<EncryptionConfig> options)
    {
        _config = options.Value;
    }

    public string Encrypt(string plainText, EncryptionKeyType keyType)
    {
        var (key, iv) = GenerateKeyAndIv(GetKeySeed(keyType));

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var base64 = Convert.ToBase64String(encryptedBytes);

        return $"enc:{base64}";
    }

    public string Decrypt(string encryptedText, EncryptionKeyType keyType)
    {
        var (key, iv) = GenerateKeyAndIv(GetKeySeed(keyType));

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private string GetKeySeed(EncryptionKeyType keyType)
    {
        return keyType switch
        {
            EncryptionKeyType.Mobile => _config.MobileCustomAuthKey,
            _ => _config.ApiIntegrationEncryptionKey
        };
    }

    private static (byte[] Key, byte[] IV) GenerateKeyAndIv(string seed)
    {
        var key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(seed));
        var iv = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(seed));
        return (key, iv);
    }
}
