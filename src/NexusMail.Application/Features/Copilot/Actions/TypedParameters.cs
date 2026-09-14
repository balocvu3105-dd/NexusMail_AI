using System;
using System.ComponentModel.DataAnnotations;

namespace NexusMail.Application.Features.Copilot.Actions;

public record CreateTaskParameters
{
    [Required]
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public DateTimeOffset? DueAt { get; init; }
}

public record ApplyLabelParameters
{
    [Required]
    public string EmailId { get; init; } = string.Empty;

    [Required]
    public string Label { get; init; } = string.Empty;
}
