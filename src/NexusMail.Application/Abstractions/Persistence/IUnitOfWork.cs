using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
