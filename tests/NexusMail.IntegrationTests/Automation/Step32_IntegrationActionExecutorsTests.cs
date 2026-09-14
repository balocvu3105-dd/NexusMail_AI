using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Abstractions.Integration;
using NexusMail.Automation.Actions;
using NexusMail.Contracts.Automation;
using NexusMail.Domain.Automation.Enums;
using Xunit;

namespace NexusMail.IntegrationTests.Automation;

public class Step32_IntegrationActionExecutorsTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>
{
    private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;

    public Step32_IntegrationActionExecutorsTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CallWebhook_WithValidUrlAnd2xx_ShouldReturnSuccess()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var clientFactory = new MockHttpClientFactory(handler);
        var executor = new CallWebhookActionExecutor(clientFactory);

        var context = new EvaluateRulesMessage
        {
            EmailId = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            Subject = "Test Webhook"
        };
        var parametersJson = """{ "url": "https://example.com/webhook", "method": "POST" }""";

        // Act
        var result = await executor.ExecuteAsync("test-key", parametersJson, context);

        // Assert
        result.Should().Be(ActionResult.Success);
        handler.Requests.Should().ContainSingle();
        var request = handler.Requests[0];
        request.RequestUri!.ToString().Should().Be("https://example.com/webhook");
        request.Headers.GetValues("Idempotency-Key").First().Should().Be("test-key");
    }

    [Theory]
    [InlineData("http://example.com", HttpStatusCode.BadRequest, ActionResult.PermanentFailure)] // 400
    [InlineData("https://example.com", HttpStatusCode.InternalServerError, ActionResult.TransientFailure)] // 500
    [InlineData("https://example.com", HttpStatusCode.BadGateway, ActionResult.TransientFailure)] // 502
    [InlineData("https://example.com", HttpStatusCode.NotFound, ActionResult.PermanentFailure)] // 404
    public async Task CallWebhook_WithHttpErrors_ShouldMapResultCorrectly(string url, HttpStatusCode statusCode, ActionResult expectedResult)
    {
        // Arrange
        var handler = new MockHttpMessageHandler(statusCode);
        var clientFactory = new MockHttpClientFactory(handler);
        var executor = new CallWebhookActionExecutor(clientFactory);
        var context = new EvaluateRulesMessage { EmailId = Guid.NewGuid() };
        var parametersJson = $$"""{ "url": "{{url}}", "method": "POST" }""";

        // Act
        var result = await executor.ExecuteAsync("test-key", parametersJson, context);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task CallWebhook_WithMalformedUrl_ShouldReturnPermanentFailure_WithoutSendingRequest()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var clientFactory = new MockHttpClientFactory(handler);
        var executor = new CallWebhookActionExecutor(clientFactory);
        var context = new EvaluateRulesMessage { EmailId = Guid.NewGuid() };
        
        var parametersJson = """{ "url": "not-a-valid-url" }""";

        // Act
        var result = await executor.ExecuteAsync("key", parametersJson, context);

        // Assert
        result.Should().Be(ActionResult.PermanentFailure);
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task CallWebhook_WithCancellation_ShouldPropagateException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, delay: TimeSpan.FromMilliseconds(500));
        var clientFactory = new MockHttpClientFactory(handler);
        var executor = new CallWebhookActionExecutor(clientFactory);
        var context = new EvaluateRulesMessage { EmailId = Guid.NewGuid() };
        var parametersJson = """{ "url": "https://example.com", "method": "GET" }""";
        
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act
        Func<Task> act = async () => await executor.ExecuteAsync("key", parametersJson, context, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task CreateTask_WithValidParams_ShouldReturnSuccessAndPassIdempotencyKey()
    {
        // Arrange
        var fakeProvider = new FakeTaskProvider();
        var executor = new CreateTaskActionExecutor(fakeProvider);
        var context = new EvaluateRulesMessage { EmailId = Guid.NewGuid() };
        var parametersJson = """{ "provider": "Jira", "title": "Review Email", "assignee": "user1" }""";

        // Act
        var result = await executor.ExecuteAsync("task-key", parametersJson, context);

        // Assert
        result.Should().Be(ActionResult.Success);
        fakeProvider.CreatedTasks.Should().ContainSingle();
        var task = fakeProvider.CreatedTasks[0];
        task.Parameters.Title.Should().Be("Review Email");
        task.Parameters.Assignee.Should().Be("user1");
        task.IdempotencyKey.Should().Be("task-key");
    }

    [Fact]
    public async Task SaveAttachments_WithValidData_ShouldDownloadAndSave()
    {
        // Arrange
        var fakeSource = new FakeAttachmentSource();
        var fakeStorage = new FakeAttachmentStorageProvider();
        var executor = new SaveAttachmentsActionExecutor(fakeSource, fakeStorage);
        
        var context = new EvaluateRulesMessage 
        { 
            EmailId = Guid.NewGuid(), 
            WorkspaceId = Guid.NewGuid(),
            HasAttachments = true 
        };
        var parametersJson = """{ "provider": "S3", "destinationPath": "/backup/emails" }""";

        // Act
        var result = await executor.ExecuteAsync("save-key", parametersJson, context);

        // Assert
        result.Should().Be(ActionResult.Success);
        fakeSource.GetAttachmentsCallCount.Should().Be(1);
        fakeStorage.SavedAttachments.Should().HaveCount(2); // Fake returns 2 attachments

        var firstSaved = fakeStorage.SavedAttachments[0];
        firstSaved.DestinationPath.Should().Be("/backup/emails");
        firstSaved.IdempotencyKey.Should().Contain("save-key:");
    }

    // --- Fakes & Mocks ---

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly TimeSpan _delay;
        public List<HttpRequestMessage> Requests { get; } = new();

        public MockHttpMessageHandler(HttpStatusCode statusCode, TimeSpan delay = default)
        {
            _statusCode = statusCode;
            _delay = delay;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (_delay != default)
            {
                await Task.Delay(_delay, cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new HttpResponseMessage(_statusCode);
        }
    }

    private class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly MockHttpMessageHandler _handler;

        public MockHttpClientFactory(MockHttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler);
        }
    }

    private class FakeTaskProvider : ITaskManagementProvider
    {
        public List<(CreateTaskParameters Parameters, string IdempotencyKey)> CreatedTasks { get; } = new();

        public Task<string> CreateTaskAsync(CreateTaskParameters parameters, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            CreatedTasks.Add((parameters, idempotencyKey));
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }

    private class FakeAttachmentSource : IAttachmentSource
    {
        public int GetAttachmentsCallCount { get; private set; }

        public Task<IReadOnlyList<AttachmentData>> GetAttachmentsAsync(Guid workspaceId, Guid emailId, CancellationToken cancellationToken = default)
        {
            GetAttachmentsCallCount++;
            var list = new List<AttachmentData>
            {
                new AttachmentData(Guid.NewGuid(), "file1.pdf", new MemoryStream(Encoding.UTF8.GetBytes("test data 1"))),
                new AttachmentData(Guid.NewGuid(), "file2.png", new MemoryStream(Encoding.UTF8.GetBytes("test data 2")))
            };
            return Task.FromResult<IReadOnlyList<AttachmentData>>(list);
        }
    }

    private class FakeAttachmentStorageProvider : IAttachmentStorageProvider
    {
        public List<(AttachmentData Attachment, string DestinationPath, string IdempotencyKey)> SavedAttachments { get; } = new();

        public Task<bool> SaveAsync(AttachmentData attachment, string destinationPath, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            SavedAttachments.Add((attachment, destinationPath, idempotencyKey));
            return Task.FromResult(true);
        }
    }
}
