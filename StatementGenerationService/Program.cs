using Amazon.DynamoDBv2;
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

builder.Services.AddSingleton<IAmazonDynamoDB>(sp => new AmazonDynamoDBClient(dynamoDbConfig));

builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
builder.Services.AddScoped<IReportGenerator, StatementGenerator>();

var host = builder.Build();
host.Run();
