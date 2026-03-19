using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using StackExchange.Redis;
using StatementGenerationService.Configuration;
using StatementGenerationService.Services.Interfaces;

namespace StatementGenerationService.Services;

public class RedisQueueConsumer<T> : IQueueConsumerService<T>
{
    private readonly ILogger<RedisQueueConsumer<T>> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly string _queueKey;

    public RedisQueueConsumer(ILogger<RedisQueueConsumer<T>> logger, IConnectionMultiplexer redis, RedisQueueSettings settings)
    {
        _logger = logger;
        _redis = redis;
        _queueKey = settings.AccountQueueKey;
    }

    public async Task<T?> DequeueAsync(CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();

        var item = await db.ListLeftPopAsync(_queueKey);
        
        _logger.LogInformation("Dequeued item: {Item}", item.ToString());
        return JsonSerializer.Deserialize<T>(item!);
        
    }
}