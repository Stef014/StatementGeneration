namespace MonthlyAccountProcessingService.Configuration;

public sealed class HangfireSettings
{
    public const string SectionName = "Hangfire";

    public string ConnectionString { get; init; } = string.Empty;
    public string RecurringJobId { get; init; } = "monthly-statement-proccessing";
    public string CronExpression { get; init; } = "0 0 1 * *";
    public string TimeZoneId { get; init; } = "Africa/Johannesburg";
}