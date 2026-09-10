namespace NexusMail.Application.Features.Email.Commands.SyncEmails;

public record SyncEmailsCommand() : ICommand<Result<Guid>>;
