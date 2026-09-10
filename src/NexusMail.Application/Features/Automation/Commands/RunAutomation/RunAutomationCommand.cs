namespace NexusMail.Application.Features.Automation.Commands.RunAutomation;

public record RunAutomationCommand() : ICommand<Result<Guid>>;
