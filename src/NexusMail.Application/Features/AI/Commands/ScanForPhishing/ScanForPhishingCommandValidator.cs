using FluentValidation;

namespace NexusMail.Application.Features.AI.Commands.ScanForPhishing;

public class ScanForPhishingCommandValidator : AbstractValidator<ScanForPhishingCommand>
{
    public ScanForPhishingCommandValidator()
    {
    }
}
