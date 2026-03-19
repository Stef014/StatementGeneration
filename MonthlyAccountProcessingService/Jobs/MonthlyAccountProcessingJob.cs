namespace MonthlyAccountProcessingService.Jobs;

public sealed class MonthlyAccountProcessingJob
{
    private readonly ILogger<MonthlyAccountProcessingJob> _logger;

    public MonthlyAccountProcessingJob(ILogger<MonthlyAccountProcessingJob> logger)
    {
        _logger = logger;
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Monthly account processing triggered at {TriggeredAt}.",
            DateTimeOffset.Now);

        return Task.CompletedTask;
    }
}