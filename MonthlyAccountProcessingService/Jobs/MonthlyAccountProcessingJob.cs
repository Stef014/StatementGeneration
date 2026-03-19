using System.Threading.Channels;
using System.Threading.Tasks;
using System.Collections.Generic;

using AccountsStatementsData;
using AccountsStatementsData.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MonthlyAccountProcessingService.Services.Interfaces;

namespace MonthlyAccountProcessingService.Jobs;

public sealed class MonthlyAccountProcessingJob
{
    private readonly ILogger<MonthlyAccountProcessingJob> _logger;

    private readonly AppDbContext _appDbContext;
    private readonly IQueueService<Account> _queueService;

    public MonthlyAccountProcessingJob(AppDbContext appDbContext, ILogger<MonthlyAccountProcessingJob> logger, IQueueService<Account> queueService)
    {
        _appDbContext = appDbContext;
        _logger = logger;
        _queueService = queueService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Monthly account processing triggered at {TriggeredAt}.",
            DateTimeOffset.Now);

        var channel = Channel.CreateBounded<Account>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });

        var producer = Task.Run(async () => {
            await foreach (var account in _appDbContext.Accounts.AsNoTracking().AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                await channel.Writer.WriteAsync(account, cancellationToken);
            }
        });

        var consumer = Task.Run(async () => {
            const int batchSize = 10;
            var batch = new List<Account>(batchSize);

            while (await channel.Reader.WaitToReadAsync(cancellationToken)) 
            {
                while (batch.Count < batchSize && channel.Reader.TryRead(out var account))
                {
                    batch.Add(account);
                }

                if (batch.Count > 0)
                {
                    await _queueService.SendBatchAsync(batch, cancellationToken);
                    batch.Clear();
                }
            }
        });

        await Task.WhenAll(producer, consumer);
    }
}