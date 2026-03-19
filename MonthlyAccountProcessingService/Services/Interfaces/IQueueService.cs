namespace MonthlyAccountProcessingService.Services.Interfaces;

public interface IQueueService<T>
{
    Task SendBatchAsync(IEnumerable<T> items, CancellationToken cancellationToken);
}