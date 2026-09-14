using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexusMail.Application.Features.Email.Abstractions;

namespace NexusMail.Infrastructure.Email.Providers.Fake;

public sealed class FakeEmailProvider : IEmailProvider
{
    private readonly List<ProviderMessage> _fakeMessages = new();
    
    public FakeEmailProvider()
    {
        GenerateFakeMessages();
    }

    public Task<ProviderProfile> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderProfile("test@nexusmail.ai", "1000"));
    }

    public Task<IReadOnlyList<ProviderLabel>> GetLabelsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ProviderLabel>>(new List<ProviderLabel>
        {
            new("INBOX", "INBOX"),
            new("SENT", "SENT")
        });
    }

    public Task<ProviderMessagePage> GetMessagesAsync(string accessToken, string? pageToken = null, string? cursor = null, CancellationToken cancellationToken = default)
    {
        // For MVP, just return all 50 in one page
        var summaries = _fakeMessages.Select(m => new ProviderMessageSummary(m.Id)).ToList();
        return Task.FromResult(new ProviderMessagePage(summaries, null, "H100"));
    }

    public Task<ProviderMessage> GetMessageAsync(string accessToken, string messageId, CancellationToken cancellationToken = default)
    {
        var message = _fakeMessages.FirstOrDefault(m => m.Id == messageId);
        if (message == null)
            throw new KeyNotFoundException($"Message {messageId} not found");
            
        return Task.FromResult(message);
    }

    public Task<ProviderTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderTokens("fake-access-token", 3600));
    }

    private void GenerateFakeMessages()
    {
        _fakeMessages.Add(new ProviderMessage(
            Id: $"msg_test_{Guid.NewGuid():N}",
            ThreadId: $"thread_test",
            Subject: "NexusMail AI Validation Test",
            Snippet: "Please review this email...",
            Body: "Please review this email and summarize the key action I need to take.\nI need to confirm the project deployment schedule by Friday and reply with my availability."
        ));
    }
}
