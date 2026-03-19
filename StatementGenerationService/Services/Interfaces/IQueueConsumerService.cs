using System.Threading;
using System.Threading.Tasks;

namespace StatementGenerationService.Services.Interfaces;

public interface IQueueConsumerService<T>
{
    Task<T?> DequeueAsync(CancellationToken cancellationToken);
}