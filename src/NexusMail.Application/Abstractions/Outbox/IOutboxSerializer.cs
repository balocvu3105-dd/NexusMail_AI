using NexusMail.Shared.Domain;

namespace NexusMail.Application.Abstractions.Outbox;

public interface IOutboxSerializer
{
    string Serialize(IDomainEvent domainEvent);
    IDomainEvent? Deserialize(string typeName, string content);
}
