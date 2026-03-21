using AccountsStatementsData;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using MonthlyAccountProcessingService.Configuration;
using MonthlyAccountProcessingService.Dtos;
using MonthlyAccountProcessingService.HostedServices;
using MonthlyAccountProcessingService.Jobs;
using MonthlyAccountProcessingService.Services;
using MonthlyAccountProcessingService.Services.Interfaces;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

var accountsStatementsConnection = builder.Configuration.GetConnectionString("AccountsStatementsConnection")
	?? throw new InvalidOperationException("AccountsStatementsConnection is missing.");

builder.Services
	.AddOptions<RedisQueueSettings>()
	.Bind(builder.Configuration.GetSection(RedisQueueSettings.SectionName))
	.Validate(settings => !string.IsNullOrWhiteSpace(settings.ConnectionString), "Redis connection string is required.")
	.Validate(settings => !string.IsNullOrWhiteSpace(settings.AccountQueueKey), "Redis queue key is required.")
	.ValidateOnStart();

var redisSettings = builder.Configuration
	.GetSection(RedisQueueSettings.SectionName)
	.Get<RedisQueueSettings>()
	?? throw new InvalidOperationException("Redis settings are missing.");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisSettings.ConnectionString));

builder.Services.AddScoped<IQueueService<StatementGenerationRequestDto>, RedisQueueService<StatementGenerationRequestDto>>();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(accountsStatementsConnection));

builder.Services
	.AddOptions<HangfireSettings>()
	.Bind(builder.Configuration.GetSection(HangfireSettings.SectionName))
	.Validate(settings => !string.IsNullOrWhiteSpace(settings.ConnectionString), "Hangfire connection string is required.")
	.Validate(settings => !string.IsNullOrWhiteSpace(settings.TimeZoneId), "Hangfire time zone is required.")
	.ValidateOnStart();

var hangfireSettings = builder.Configuration
	.GetSection(HangfireSettings.SectionName)
	.Get<HangfireSettings>()
	?? throw new InvalidOperationException("Hangfire settings are missing.");

builder.Services.AddHangfire(configuration => configuration
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(hangfireSettings.ConnectionString)));

builder.Services.AddHangfireServer();
builder.Services.AddScoped<MonthlyAccountProcessingJob>();
builder.Services.AddHostedService<RecurringJobsHostedService>();

var host = builder.Build();
host.Run();
