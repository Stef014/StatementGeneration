using Hangfire;
using Microsoft.Extensions.Options;
using MonthlyAccountProcessingService.Configuration;
using MonthlyAccountProcessingService.Jobs;

namespace MonthlyAccountProcessingService.HostedServices;

public sealed class RecurringJobsHostedService : IHostedService
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly HangfireSettings _settings;
    private readonly ILogger<RecurringJobsHostedService> _logger;

    public RecurringJobsHostedService(
        IRecurringJobManager recurringJobManager,
        IOptions<HangfireSettings> settings,
        ILogger<RecurringJobsHostedService> logger)
    {
        _recurringJobManager = recurringJobManager;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_settings.TimeZoneId);

        _recurringJobManager.AddOrUpdate<MonthlyAccountProcessingJob>(
            _settings.RecurringJobId,
            job => job.RunAsync(CancellationToken.None),
            _settings.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = timeZone
            });

        _logger.LogInformation(
            "Registered recurring Hangfire job {JobId} using cron {CronExpression} in time zone {TimeZoneId}.",
            _settings.RecurringJobId,
            _settings.CronExpression,
            _settings.TimeZoneId);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}