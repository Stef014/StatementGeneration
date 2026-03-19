using Hangfire;
using Hangfire.PostgreSql;
using MonthlyAccountProcessingService.Configuration;
using MonthlyAccountProcessingService.HostedServices;
using MonthlyAccountProcessingService.Jobs;

var builder = Host.CreateApplicationBuilder(args);

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
