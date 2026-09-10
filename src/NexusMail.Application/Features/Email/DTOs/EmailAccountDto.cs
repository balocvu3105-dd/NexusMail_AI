using System;
using NexusMail.Domain.Email.Enums;

namespace NexusMail.Application.Features.Email.DTOs;

public record EmailAccountDto(
    Guid Id,
    EmailProvider Provider,
    string EmailAddress,
    EmailAccountStatus Status,
    bool SyncEnabled,
    DateTime? LastSyncAt,
    DateTime CreatedAt
);
