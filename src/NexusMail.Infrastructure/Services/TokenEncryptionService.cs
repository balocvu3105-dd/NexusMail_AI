using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Infrastructure.Services;

public sealed class TokenEncryptionService : ITokenEncryptionService
{
    private readonly string _encryptionKey;
    public string CurrentVersion => "v1";

    public TokenEncryptionService(string encryptionKey)
    {
        if (string.IsNullOrWhiteSpace(encryptionKey) || encryptionKey.Length != 32)
        {
            throw new ArgumentException("Encryption key must be a 32-character string for AES-256.", nameof(encryptionKey));
        }
        _encryptionKey = encryptionKey;
    }

    public async Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            await sw.WriteAsync(plainText.AsMemory(), cancellationToken);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public async Task<string> DecryptAsync(string cipherText, string encryptionVersion, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        if (encryptionVersion != CurrentVersion)
        {
            throw new NotSupportedException($"Encryption version {encryptionVersion} is not supported.");
        }

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
        
        var iv = new byte[aes.IV.Length];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        var encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        
        return await sr.ReadToEndAsync(cancellationToken);
    }
}
