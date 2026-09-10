using System;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Features.Email.Abstractions;

public interface IEmailTokenService
{
    Task<string> GetValidAccessTokenAsync(Guid emailAccountId, CancellationToken cancellationToken = default);
}
