using FluentValidation;

namespace NexusMail.Application.Features.Email.Queries.SearchEmails;

public class SearchEmailsQueryValidator : AbstractValidator<SearchEmailsQuery>
{
    public SearchEmailsQueryValidator()
    {
        RuleFor(x => x.QueryText).NotEmpty().WithMessage("Search query text is required.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                                .LessThanOrEqualTo(100).WithMessage("PageSize must be 100 or less.");
    }
}
