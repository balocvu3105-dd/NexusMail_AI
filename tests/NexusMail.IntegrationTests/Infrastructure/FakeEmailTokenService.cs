using System;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.IntegrationTests.Infrastructure;

public class FakeEmailTokenService : IEmailTokenService
{
    public Task<string> GetValidAccessTokenAsync(Guid emailAccountId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("fake_access_token_for_integration_tests");
    }
}
