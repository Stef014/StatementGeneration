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
using System.Text.Json;

namespace StatementGenerationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IQueueConsumerService<string> _queueConsumerService;
    private readonly IDataDecryptionService _dataDecryptionService;

    public Worker(ILogger<Worker> logger, IQueueConsumerService<string> queueConsumerService, IServiceScopeFactory scopeFactory, IDataDecryptionService dataDecryptionService)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _dataDecryptionService = dataDecryptionService;
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

                var decryptedInput = _dataDecryptionService.Decrypt(accountInput);
                var statementRequest = JsonSerializer.Deserialize<StatementGenerationRequest>(decryptedInput);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var statementJob = scope.ServiceProvider.GetRequiredService<IJob<StatementGenerationRequest>>();
                    await statementJob.ExecuteAsync(statementRequest, stoppingToken);
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
