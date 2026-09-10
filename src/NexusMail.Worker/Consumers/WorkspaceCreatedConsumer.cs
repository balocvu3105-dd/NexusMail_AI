using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NexusMail.Application.Features.Workspace.Commands.CreateDefaultResources;
using NexusMail.Domain.Workspace.Events;

namespace NexusMail.Worker.Consumers;

public class WorkspaceCreatedConsumer : IConsumer<WorkspaceCreated>
{
    private readonly ISender _sender;
    private readonly ILogger<WorkspaceCreatedConsumer> _logger;

    public WorkspaceCreatedConsumer(ISender sender, ILogger<WorkspaceCreatedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WorkspaceCreated> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing WorkspaceCreated for workspace {WorkspaceId}", context.Message.WorkspaceId);

        // 11th Commandment: Worker translates Event to Command and delegates to MediatR
        var command = new CreateDefaultWorkspaceResourcesCommand(message.WorkspaceId, message.CreatorId);
        
        await _sender.Send(command);
        
        _logger.LogInformation("Successfully dispatched CreateDefaultWorkspaceResourcesCommand");
    }
}
