using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleEmail;
using QuestPDF.Infrastructure;

using StatementGenerationService;
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

builder.Services.AddSingleton<IAmazonDynamoDB>(sp => new AmazonDynamoDBClient(credentials, dynamoDbConfig));
builder.Services.AddSingleton<IAmazonSimpleEmailService>(sp => new AmazonSimpleEmailServiceClient(credentials, mailConfig));
builder.Services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(credentials, s3Config));

builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
builder.Services.AddScoped<IFileStorageRepository, FileStorageRepository>();
builder.Services.AddScoped<IFileManagementService, FileManagementService>();
builder.Services.AddScoped<IStatementRepository, StatementRepository>();
builder.Services.AddScoped<IStatementsService, StatementsService>();
builder.Services.AddScoped<IReportGenerator, StatementGenerator>();
builder.Services.AddScoped<IMailingService, MailingService>();

var host = builder.Build();
host.Run();
