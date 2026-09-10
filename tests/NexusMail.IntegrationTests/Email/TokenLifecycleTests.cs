using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NexusMail.Application.Abstractions.Persistence;
using NexusMail.Application.Features.Email.Abstractions;
using NexusMail.Domain.Email.Entities;
using NexusMail.Domain.Email.Enums;
using NexusMail.Infrastructure.Authentication;
using NexusMail.Shared.Common;
using Xunit;

namespace NexusMail.IntegrationTests.Email;

public class TokenLifecycleTests
{
    [Fact]
    public async Task GetValidAccessTokenAsync_ValidToken_ReturnsExistingToken()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        
        var account = EmailAccount.Create(workspaceId, EmailProvider.Google, "test@test.com", "enc_access", "enc_refresh", DateTime.UtcNow.AddMinutes(10));
        
        var repoMock = new Mock<IEmailAccountRepository>();
        repoMock.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var encMock = new Mock<ITokenEncryptionService>();
        encMock.Setup(e => e.DecryptAsync("enc_access", "v1", It.IsAny<CancellationToken>())).ReturnsAsync("valid_access_token");

        var uowMock = new Mock<IUnitOfWork>();

        var service = new EmailTokenService(repoMock.Object, encMock.Object, new List<IOAuthProvider>(), uowMock.Object, NullLogger<EmailTokenService>.Instance);

        // Act
        var token = await service.GetValidAccessTokenAsync(accountId);

        // Assert
        Assert.Equal("valid_access_token", token);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // No save should happen
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ExpiredToken_RefreshesAndSaves()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        
        // Token expired 5 minutes ago
        var account = EmailAccount.Create(workspaceId, EmailProvider.Google, "test@test.com", "enc_access_old", "enc_refresh_old", DateTime.UtcNow.AddMinutes(-5));
        
        var repoMock = new Mock<IEmailAccountRepository>();
        repoMock.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var encMock = new Mock<ITokenEncryptionService>();
        encMock.Setup(e => e.DecryptAsync("enc_access_old", "v1", It.IsAny<CancellationToken>())).ReturnsAsync("expired_access_token");
        encMock.Setup(e => e.DecryptAsync("enc_refresh_old", "v1", It.IsAny<CancellationToken>())).ReturnsAsync("refresh_token_plain");
        
        encMock.Setup(e => e.EncryptAsync("new_access_token", It.IsAny<CancellationToken>())).ReturnsAsync("enc_access_new");
        encMock.Setup(e => e.EncryptAsync("new_refresh_token", It.IsAny<CancellationToken>())).ReturnsAsync("enc_refresh_new");

        var providerMock = new Mock<IOAuthProvider>();
        providerMock.SetupGet(p => p.Provider).Returns(EmailProvider.Google);
        providerMock.Setup(p => p.RefreshTokenAsync("refresh_token_plain", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new OAuthTokens("new_access_token", "new_refresh_token", DateTime.UtcNow.AddHours(1), "")));

        var uowMock = new Mock<IUnitOfWork>();

        var service = new EmailTokenService(repoMock.Object, encMock.Object, new[] { providerMock.Object }, uowMock.Object, NullLogger<EmailTokenService>.Instance);

        // Act
        var token = await service.GetValidAccessTokenAsync(accountId);

        // Assert
        Assert.Equal("new_access_token", token);
        Assert.Equal("enc_access_new", account.EncryptedAccessToken);
        Assert.Equal("enc_refresh_new", account.EncryptedRefreshToken);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
