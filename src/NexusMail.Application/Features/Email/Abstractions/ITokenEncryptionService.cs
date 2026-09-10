using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface ITokenEncryptionService
{
    string CurrentVersion { get; }

    Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken = default);
    Task<string> DecryptAsync(string cipherText, string encryptionVersion, CancellationToken cancellationToken = default);
}
