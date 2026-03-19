namespace StatementGenerationService.Jobs.Interfaces;

public interface IJob<T>
{
    Task ExecuteAsync(T input, CancellationToken cancellationToken);
}