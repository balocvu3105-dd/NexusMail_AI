using FluentValidation;

namespace NexusMail.Application.Features.Workspace.Commands.CreateWorkspace;

public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("Name contains invalid characters.");
            
        RuleFor(v => v.Plan)
            .NotEmpty().WithMessage("Plan is required.")
            .Must(plan => plan == "Free" || plan == "Pro" || plan == "Enterprise")
            .WithMessage("Plan must be 'Free', 'Pro', or 'Enterprise'.");
    }
}
