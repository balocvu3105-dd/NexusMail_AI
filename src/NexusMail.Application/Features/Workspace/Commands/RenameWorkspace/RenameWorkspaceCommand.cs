namespace NexusMail.Application.Features.Workspace.Commands.RenameWorkspace;

public record RenameWorkspaceCommand() : ICommand<Result<Guid>>;
