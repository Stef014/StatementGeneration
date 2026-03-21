using System;
using System.Threading;
using System.Threading.Tasks;
using StatementGenerationService.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using StatementGenerationService.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StatementGenerationService.Jobs.Interfaces;
using StatementGenerationService.Models;

namespace StatementGenerationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IQueueConsumerService<StatementGenerationRequest> _queueConsumerService;

    public Worker(ILogger<Worker> logger, IQueueConsumerService<StatementGenerationRequest> queueConsumerService, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _queueConsumerService = queueConsumerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                var accountInput = await _queueConsumerService.DequeueAsync(stoppingToken);

                if (accountInput is null)
                {
                    _logger.LogInformation("Waiting for account entries...");
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var statementJob = scope.ServiceProvider.GetRequiredService<IJob<StatementGenerationRequest>>();
                    await statementJob.ExecuteAsync(accountInput, stoppingToken);
                }

                _logger.LogInformation("Statement generation completed at: {time}", DateTimeOffset.Now);    
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the statement.");
                continue;
            }
            

            await Task.Delay(1000, stoppingToken);
        }
    }
}
