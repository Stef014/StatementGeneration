namespace MonthlyAccountProcessingService.Configuration;

public sealed class RedisQueueSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;
    public string AccountQueueKey { get; init; } = "monthly-statement-proccessing";
}