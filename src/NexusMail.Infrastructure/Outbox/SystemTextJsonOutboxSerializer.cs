using System;
using System.Text.Json;
using NexusMail.Application.Abstractions.Outbox;
using NexusMail.Shared.Domain;

namespace NexusMail.Infrastructure.Outbox;

public sealed class SystemTextJsonOutboxSerializer : IOutboxSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = false
    };

    public string Serialize(IDomainEvent domainEvent)
    {
        return JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _options);
    }

    public IDomainEvent? Deserialize(string typeName, string content)
    {
        Type? type = Type.GetType(typeName);
        if (type == null)
            return null;
            
        var obj = JsonSerializer.Deserialize(content, type, _options);
        return obj as IDomainEvent;
    }
}
