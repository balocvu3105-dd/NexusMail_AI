using System.Collections.Generic;
using NexusMail.Application.Abstractions.Messaging;
using NexusMail.Application.Features.Email.DTOs;
using NexusMail.Shared.Common;

using NexusMail.Shared.Pagination;

namespace NexusMail.Application.Features.Email.Queries.GetEmailAccounts;

public record GetEmailAccountsQuery(PageRequest PageRequest) : IQuery<Result<PagedResult<EmailAccountDto>>>;

