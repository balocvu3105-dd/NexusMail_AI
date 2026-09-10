namespace NexusMail.Application.Features.Workspace.Commands.DeleteWorkspace;

public record DeleteWorkspaceCommand() : ICommand<Result<Guid>>;
