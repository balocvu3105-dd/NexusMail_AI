using FluentValidation;

namespace NexusMail.Application.Features.Email.Queries.GetEmail;

public class GetEmailQueryValidator : AbstractValidator<GetEmailQuery>
{
    public GetEmailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Email ID is required.");
    }
}
