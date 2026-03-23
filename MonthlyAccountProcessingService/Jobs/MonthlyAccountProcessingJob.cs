using System.Threading.Channels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

using AccountsStatementsData;
using Microsoft.EntityFrameworkCore;
using MonthlyAccountProcessingService.Dtos;
using MonthlyAccountProcessingService.Services.Interfaces;

namespace MonthlyAccountProcessingService.Jobs;

public sealed class MonthlyAccountProcessingJob
{
    private readonly ILogger<MonthlyAccountProcessingJob> _logger;

    private readonly AppDbContext _appDbContext;
    private readonly IQueueService<string> _queueService;
    private readonly IDataEncryptionService _dataEncryptionService;

    public MonthlyAccountProcessingJob(AppDbContext appDbContext, ILogger<MonthlyAccountProcessingJob> logger, IQueueService<string> queueService, IDataEncryptionService dataEncryptionService)
    {
        _appDbContext = appDbContext;
        _logger = logger;
        _queueService = queueService;
        _dataEncryptionService = dataEncryptionService;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Monthly account processing triggered at {TriggeredAt}.",
            DateTimeOffset.Now);

        var (startTimestamp, endTimestamp) = GetPreviousMonthPeriod(DateTimeOffset.UtcNow);

        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });

        var producer = Task.Run(async () => {
            try
            {
                await foreach (var account in _appDbContext.Accounts.AsNoTracking().AsAsyncEnumerable().WithCancellation(cancellationToken))
                {
                    var requestDto = new StatementGenerationRequestDto
                    {
                        AccountId = account.AccountId,
                        AccountHolderName = account.AccountHolderName,
                        AccountHolderEmailAddress = account.AccountHolderEmailAddress,
                        StartTimestamp = startTimestamp,
                        EndTimestamp = endTimestamp
                    };

                    var encryptedRequest = _dataEncryptionService.Encrypt(JsonSerializer.Serialize(requestDto));

                    await channel.Writer.WriteAsync(encryptedRequest, cancellationToken);
                }
            }
            finally
            {
                channel.Writer.TryComplete();
            }
        });

        var consumer = Task.Run(async () => {
            const int batchSize = 10;
            var batch = new List<string>(batchSize);

            while (await channel.Reader.WaitToReadAsync(cancellationToken)) 
            {
                while (batch.Count < batchSize && channel.Reader.TryRead(out var encryptedRequest))
                {
                    batch.Add(encryptedRequest);
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

    private static (long StartTimestamp, long EndTimestamp) GetPreviousMonthPeriod(DateTimeOffset currentTime)
    {
        var currentMonthStart = new DateTimeOffset(currentTime.Year, currentTime.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart.AddMilliseconds(-1);

        return (previousMonthStart.ToUnixTimeMilliseconds(), previousMonthEnd.ToUnixTimeMilliseconds());
    }
}