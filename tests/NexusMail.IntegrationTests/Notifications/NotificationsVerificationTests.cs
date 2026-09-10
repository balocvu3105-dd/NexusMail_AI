using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using NexusMail.Application.Features.Identity.Commands.Register;
using NexusMail.Application.Features.Identity.Commands.Login;
using NexusMail.Application.Features.Workspace.Commands.CreateWorkspace;
using NexusMail.Domain.Workspace.Repositories;
using NexusMail.Contracts.Email;
using MassTransit;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Notification.Domain;
using System.Linq;
using NexusMail.Application.Features.Notification.DTOs;
using MassTransit.Testing;

namespace NexusMail.IntegrationTests.Notifications
{
    public class RealWorkspaceWebApplicationFactory : CustomWebApplicationFactory<NexusMail.API.ApiMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType == typeof(IWorkspaceRepository)).ToList();
                foreach (var d in descriptors) services.Remove(d);
                
                services.AddScoped<IWorkspaceRepository, NexusMail.Infrastructure.Persistence.Repositories.WorkspaceRepository>();
            });
        }
    }

    [Collection("Integration")]
    public class NotificationsVerificationTests : IClassFixture<RealWorkspaceWebApplicationFactory>
    {
        private readonly RealWorkspaceWebApplicationFactory _factory;

        public NotificationsVerificationTests(RealWorkspaceWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private async Task<(HttpClient Client, string Token, Guid UserId, Guid WorkspaceId)> CreateAuthenticatedClientAndWorkspaceAsync(string email)
        {
            var client = _factory.CreateClient();
            
            // Register with retry
            HttpResponseMessage registerResponse = null!;
            for (int i = 0; i < 5; i++)
            {
                registerResponse = await client.PostAsJsonAsync("/api/v1/identity/register", new RegisterCommand(email, "Test", "User", "Test@123!"));
                if (registerResponse.IsSuccessStatusCode) break;
                if (registerResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) await Task.Delay(2000);
            }
            registerResponse.EnsureSuccessStatusCode();

            // Delay to avoid LoginPolicy rate limit 503
            await Task.Delay(1000);

            // Login with retry for rate limiting
            HttpResponseMessage loginResponse = null!;
            for (int i = 0; i < 5; i++)
            {
                loginResponse = await client.PostAsJsonAsync("/api/v1/identity/login", new LoginCommand(email, "Test@123!", "TestDevice", "127.0.0.1", "TestAgent"));
                if (loginResponse.IsSuccessStatusCode) break;
                if (loginResponse.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) await Task.Delay(2000);
            }
            loginResponse.EnsureSuccessStatusCode();
            var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
            var token = loginData!.AccessToken;

            // Setup Auth Header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create Workspace
            var wsResponse = await client.PostAsJsonAsync("/api/v1/workspaces", new CreateWorkspaceCommand($"Workspace {Guid.NewGuid():N}", "Free"));
            if (!wsResponse.IsSuccessStatusCode)
            {
                var errorBody = await wsResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create workspace. Status: {wsResponse.StatusCode}. Body: {errorBody}");
            }
            
            var workspaceData = await wsResponse.Content.ReadFromJsonAsync<ApiResponse<WorkspaceResponse>>();
            var workspaceId = workspaceData!.Data.Id;
            workspaceId.Should().NotBe(Guid.Empty, "Workspace creation should return a valid Guid");

            return (client, token, loginData.UserId, workspaceId);
        }

        [Fact]
        public async Task REST_GetNotifications_WithWorkspaceIsolation_ShouldWork()
        {
            // Arrange
            var userA = await CreateAuthenticatedClientAndWorkspaceAsync($"usera_search_{Guid.NewGuid():N}@test.com");
            var userB = await CreateAuthenticatedClientAndWorkspaceAsync($"userb_search_{Guid.NewGuid():N}@test.com");

            // Seed a notification for User A
            using (var scope = _factory.Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var notif = NotificationItem.Create(userA.WorkspaceId, NotificationType.EmailReceived, "e1", "T1", "M1", "{}", "/");
                await repo.AddAsync(notif);
                await uow.SaveChangesAsync();
            }

            // Act - User A
            var requestA = new HttpRequestMessage(HttpMethod.Get, "/api/v1/notifications");
            requestA.Headers.Add("X-Workspace-Id", userA.WorkspaceId.ToString());
            var responseA = await userA.Client.SendAsync(requestA);
            
            // Assert - User A
            responseA.EnsureSuccessStatusCode();
            var notifsA = await responseA.Content.ReadFromJsonAsync<System.Collections.Generic.List<NotificationDto>>();
            notifsA.Should().Contain(n => n.Title == "T1");

            // Act - User B tries to read User A's workspace
            var requestB = new HttpRequestMessage(HttpMethod.Get, "/api/v1/notifications");
            requestB.Headers.Add("X-Workspace-Id", userA.WorkspaceId.ToString()); // trying to access A
            var responseB = await userB.Client.SendAsync(requestB);
            
            // Assert - User B
            responseB.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task REST_PatchNotification_MarkAsRead_ShouldWorkAndPreventCrossWorkspace()
        {
            var userA = await CreateAuthenticatedClientAndWorkspaceAsync($"usera_patch_{Guid.NewGuid():N}@test.com");
            var userB = await CreateAuthenticatedClientAndWorkspaceAsync($"userb_patch_{Guid.NewGuid():N}@test.com");
            
            Guid notifId;
            using (var scope = _factory.Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var notif = NotificationItem.Create(userA.WorkspaceId, NotificationType.EmailReceived, "e2", "T2", "M2", "{}", "/");
                await repo.AddAsync(notif);
                await uow.SaveChangesAsync();
                notifId = notif.Id;
            }

            // Act - User B tries to patch User A's notification
            var requestB = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/notifications/{notifId}/read");
            requestB.Headers.Add("X-Workspace-Id", userB.WorkspaceId.ToString()); 
            var responseB = await userB.Client.SendAsync(requestB);
            
            // Assert - User B
            responseB.StatusCode.Should().Be(HttpStatusCode.BadRequest); // NullValue mapped to 400

            // Act - User A patches their own
            var requestA = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/notifications/{notifId}/read");
            requestA.Headers.Add("X-Workspace-Id", userA.WorkspaceId.ToString()); 
            var responseA = await userA.Client.SendAsync(requestA);
            
            // Assert - User A
            responseA.EnsureSuccessStatusCode();
            
            using (var scope = _factory.Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var notifs = await repo.GetByWorkspaceAsync(userA.WorkspaceId, false, 100);
                notifs.First(n => n.Id == notifId).IsRead.Should().BeTrue();
            }
        }

        [Fact]
        public async Task SignalR_Authorization_ValidAndInvalidWorkspace()
        {
            var userA = await CreateAuthenticatedClientAndWorkspaceAsync($"usera_sig_{Guid.NewGuid():N}@test.com");
            var userB = await CreateAuthenticatedClientAndWorkspaceAsync($"userb_sig_{Guid.NewGuid():N}@test.com");

            var server = _factory.Server;
            
            // We need to use TestServer's HttpMessageHandler for SignalR Client
            var handler = server.CreateHandler();

            var hubUrlA = $"http://localhost/hub/notifications?workspaceId={userA.WorkspaceId}";
            var connectionA = new HubConnectionBuilder()
                .WithUrl(hubUrlA, options => 
                {
                    options.HttpMessageHandlerFactory = _ => handler;
                    options.AccessTokenProvider = () => Task.FromResult(userA.Token)!;
                })
                .Build();

            // Should connect successfully
            await connectionA.StartAsync();
            connectionA.State.Should().Be(HubConnectionState.Connected);
            await connectionA.StopAsync();

            // User B tries to connect to User A's workspace
            var hubUrlB = $"http://localhost/hub/notifications?workspaceId={userA.WorkspaceId}";
            var connectionB = new HubConnectionBuilder()
                .WithUrl(hubUrlB, options => 
                {
                    options.HttpMessageHandlerFactory = _ => handler;
                    options.AccessTokenProvider = () => Task.FromResult(userB.Token)!; // B's token, A's workspace
                })
                .Build();

            // Should fail or disconnect immediately
            Exception? closedException = null;
            connectionB.Closed += (ex) => 
            {
                closedException = ex;
                return Task.CompletedTask;
            };

            try
            {
                await connectionB.StartAsync();
                await Task.Delay(500); // Allow time for server to disconnect
            }
            catch (Exception ex)
            {
                closedException = ex;
            }

            connectionB.State.Should().Be(HubConnectionState.Disconnected);
            closedException.Should().NotBeNull();
        }

        [Fact]
        public async Task Consumer_RealtimeDelivery_And_Idempotency()
        {
            var userA = await CreateAuthenticatedClientAndWorkspaceAsync($"usera_cons_{Guid.NewGuid():N}@test.com");
            
            var server = _factory.Server;
            var handler = server.CreateHandler();

            var hubUrl = $"http://localhost/hub/notifications?workspaceId={userA.WorkspaceId}";
            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options => 
                {
                    options.HttpMessageHandlerFactory = _ => handler;
                    options.AccessTokenProvider = () => Task.FromResult(userA.Token)!;
                })
                .Build();

            NotificationDto receivedDto = null;
            connection.On<NotificationDto>("ReceiveNotification", (dto) => 
            {
                receivedDto = dto;
            });

            await connection.StartAsync();

            // Publish Message
            var messageId = Guid.NewGuid().ToString();
            var emailMsg = new EmailReceivedMessage 
            {
                EmailId = Guid.NewGuid(),
                WorkspaceId = userA.WorkspaceId,
                Sender = "test@sender.com",
                SenderName = "Test Sender",
                Subject = "Hello World",
                CorrelationId = messageId
            };

            using var scope = _factory.Services.CreateScope();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            
            try 
            {
                await publishEndpoint.Publish(emailMsg);

                // Wait for consumer and broadcast
                await Task.Delay(2000);

                receivedDto.Should().NotBeNull();
                receivedDto!.Title.Should().Be("New Email from Test Sender");

                // Test Idempotency - publish exactly same message
                await publishEndpoint.Publish(emailMsg);
                await Task.Delay(1000); // give time to process

                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var allNotifs = await repo.GetByWorkspaceAsync(userA.WorkspaceId, false, 100);
                
                // Should only have 1 with this messageId
                allNotifs.Count(n => n.SourceEventId == messageId).Should().Be(1);
            }
            finally
            {
                await connection.StopAsync();
            }
        }
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }

    public class WorkspaceResponse
    {
        public Guid Id { get; set; }
    }

    public record ApiResponse<T>(T Data, string TraceId, DateTimeOffset Timestamp);
}
