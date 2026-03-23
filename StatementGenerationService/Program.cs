using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleEmail;
using QuestPDF.Infrastructure;
using StackExchange.Redis;

using StatementGenerationService;
using StatementGenerationService.Configuration;
using StatementGenerationService.Jobs;
using StatementGenerationService.Jobs.Interfaces;
using StatementGenerationService.Models;
using StatementGenerationService.Services;
using StatementGenerationService.Services.Interfaces;
using StatementGenerationService.Repositories;
using StatementGenerationService.Repositories.Interfaces;


QuestPDF.Settings.License = LicenseType.Community;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var dynamoDbConfig = new AmazonDynamoDBConfig
{
    ServiceURL = builder.Configuration["DynamoDbServer"]
};

var credentials = new Amazon.Runtime.BasicAWSCredentials(builder.Configuration["AwsAccessKey"], builder.Configuration["AwsSecretKey"]);

var s3Config = new AmazonS3Config
{
    ServiceURL = builder.Configuration["AwsServiceUrl"],
    ForcePathStyle = true
};

var mailConfig = new AmazonSimpleEmailServiceConfig
{
    ServiceURL = builder.Configuration["AwsServiceUrl"],
    AuthenticationRegion = builder.Configuration["AwsRegion"],
};

var redisSettings = builder.Configuration
    .GetSection(RedisQueueSettings.SectionName)
    .Get<RedisQueueSettings>()
    ?? throw new InvalidOperationException("Redis settings are missing.");

if (string.IsNullOrWhiteSpace(redisSettings.ConnectionString))
{
    throw new InvalidOperationException("Redis connection string is missing.");
}

builder.Services.AddSingleton<IAmazonDynamoDB>(sp => new AmazonDynamoDBClient(credentials, dynamoDbConfig));
builder.Services.AddSingleton<IAmazonSimpleEmailService>(sp => new AmazonSimpleEmailServiceClient(credentials, mailConfig));
builder.Services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(credentials, s3Config));
builder.Services.AddSingleton(redisSettings);
var redis = ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

builder.Services.AddSingleton<IQueueConsumerService<string>, RedisQueueConsumer<string>>();
builder.Services.AddSingleton<IDataDecryptionService, DataDecryptionService>();

builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
builder.Services.AddScoped<IFileStorageRepository, FileStorageRepository>();
builder.Services.AddScoped<IFileManagementService, FileManagementService>();
builder.Services.AddScoped<IStatementRepository, StatementRepository>();
builder.Services.AddScoped<IStatementsService, StatementsService>();
builder.Services.AddScoped<IReportGenerator, StatementGenerator>();
builder.Services.AddScoped<IMailingService, MailingService>();
builder.Services.AddScoped<IJob<StatementGenerationRequest>, StatementGenerationJob>();

var host = builder.Build();

host.Run();
