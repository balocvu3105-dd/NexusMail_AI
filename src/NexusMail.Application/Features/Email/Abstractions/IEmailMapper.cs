namespace NexusMail.Application.Features.Email.Abstractions;

public interface IEmailMapper
{
    // Simplified for now - will be expanded when actual Domain entities for Emails are introduced
    object MapToDomainMessage(ProviderMessage providerMessage, System.Guid emailAccountId);
}
