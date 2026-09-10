using System.Threading.Tasks;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NexusMail.Application.Features.Email.Commands.ConnectEmailAccount;
using NexusMail.Worker.AI.Consumers;
using Xunit;
using FluentAssertions;
using MassTransit;
using NexusMail.Contracts.AI;
using System.Linq;

namespace NexusMail.IntegrationTests.Email
{
    public class MvpValidationTests : IClassFixture<CustomWebApplicationFactory<NexusMail.API.ApiMarker>>
    {
        private readonly CustomWebApplicationFactory<NexusMail.API.ApiMarker> _factory;

        public MvpValidationTests(CustomWebApplicationFactory<NexusMail.API.ApiMarker> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ConnectEmailAccount_ShouldTriggerAIAndAutomationWorkers()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;
            var testHarness = services.GetRequiredService<ITestHarness>();
            await testHarness.Start();

            try 
            {
                // Act
                var sender = services.GetRequiredService<MediatR.ISender>();
                var command = new ConnectEmailAccountCommand(NexusMail.Domain.Email.Enums.EmailProvider.Fake, "code", "http://local");
                var result = await sender.Send(command);

                // Assert
                result.IsSuccess.Should().BeTrue();

                // Wait for background tasks outbox dispatching (approximate)
                await Task.Delay(1000);

                var publishedMessages = testHarness.Published.Select<SummaryRequestedMessage>().ToList();
                publishedMessages.Should().NotBeNull();
            }
            finally
            {
                await testHarness.Stop();
            }
        }
    }
}
