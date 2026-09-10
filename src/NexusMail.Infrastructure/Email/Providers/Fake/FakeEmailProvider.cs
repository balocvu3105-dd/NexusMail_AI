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
        int idCounter = 1;

        // Khách hàng cần phản hồi (5 emails)
        for (int i = 1; i <= 5; i++)
        {
            _fakeMessages.Add(new ProviderMessage(
                Id: $"msg_{idCounter++}",
                ThreadId: $"thread_{idCounter}",
                Subject: $"Urgent: Issue with production deployment #{i}",
                Snippet: $"Hello team, we are facing issues in production. Please check...",
                Body: $"Hello team,\n\nWe are facing critical issues in production after the latest deployment. The system is returning 500 errors. Please investigate immediately before 17:00.\n\nThanks,\nClient {i}"
            ));
        }

        // Công việc nội bộ (8 emails)
        for (int i = 1; i <= 8; i++)
        {
            _fakeMessages.Add(new ProviderMessage(
                Id: $"msg_{idCounter++}",
                ThreadId: $"thread_{idCounter}",
                Subject: $"Internal: Architecture review for sprint {i}",
                Snippet: $"Hi team, let's meet to discuss the architecture...",
                Body: $"Hi team,\n\nPlease find attached the architecture review document for the upcoming sprint. We need to finalize the database schema and API contracts by Friday.\n\nBest,\nTech Lead"
            ));
        }

        // Newsletter (12 emails)
        for (int i = 1; i <= 12; i++)
        {
            _fakeMessages.Add(new ProviderMessage(
                Id: $"msg_{idCounter++}",
                ThreadId: $"thread_{idCounter}",
                Subject: $"Weekly Tech Digest - Issue {i * 10}",
                Snippet: $"Top stories in tech this week...",
                Body: $"Welcome to the Weekly Tech Digest!\n\nHere are the top stories:\n1. AI takes over the world.\n2. New JavaScript framework released.\n3. The end of passwords.\n\nClick here to read more."
            ));
        }

        // Marketing (10 emails)
        for (int i = 1; i <= 10; i++)
        {
            _fakeMessages.Add(new ProviderMessage(
                Id: $"msg_{idCounter++}",
                ThreadId: $"thread_{idCounter}",
                Subject: $"Limited time offer: Get 50% off {i}!",
                Snippet: $"Don't miss out on this amazing deal...",
                Body: $"Hey there,\n\nDon't miss out on our biggest sale of the year. Get 50% off all products. Use code SALE50 at checkout.\n\nHurry, offer ends soon!\n\nMarketing Team"
            ));
        }

        // Notification (10 emails)
        for (int i = 1; i <= 10; i++)
        {
            _fakeMessages.Add(new ProviderMessage(
                Id: $"msg_{idCounter++}",
                ThreadId: $"thread_{idCounter}",
                Subject: $"GitHub: New PR opened by developer{i}",
                Snippet: $"A new pull request was opened in NexusMail-AI...",
                Body: $"Hello,\n\nA new pull request was opened by developer{i} in the NexusMail-AI repository.\n\nTitle: Fix bug in EmailSyncWorker\n\nPlease review it."
            ));
        }

        // Misc (5 emails)
        for (int i = 1; i <= 5; i++)
        {
            _fakeMessages.Add(new ProviderMessage(
                Id: $"msg_{idCounter++}",
                ThreadId: $"thread_{idCounter}",
                Subject: $"Random question {i}",
                Snippet: $"Hey, do you know if...",
                Body: $"Hey,\n\nJust a quick question. Do you know if we have off next Monday? I forgot to check the holiday calendar.\n\nThanks,\nColleague"
            ));
        }
    }
}
