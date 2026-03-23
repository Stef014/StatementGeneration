using System.Text.Json;
using Microsoft.Extensions.Options;
using MonthlyAccountProcessingService.Configuration;
using MonthlyAccountProcessingService.Services.Interfaces;
using StackExchange.Redis;

namespace MonthlyAccountProcessingService.Services;

public class RedisQueueService<T> : IQueueService<T>
{
    private readonly IDatabase _db;
    private readonly string _queueKey;

    public RedisQueueService(
        IConnectionMultiplexer redisConnection,
        IOptions<RedisQueueSettings> settings)
    {
        _db = redisConnection.GetDatabase();
        _queueKey = settings.Value.AccountQueueKey;
    }

    public async Task SendBatchAsync(IEnumerable<T> items, CancellationToken cancellationToken)
    {
        var values = items
            .Select(item => typeof(T) == typeof(string) 
                ? (RedisValue)item!.ToString()! 
                : (RedisValue)JsonSerializer.Serialize(item))
            .ToArray();

        await _db.ListRightPushAsync(_queueKey, values);
    }
}