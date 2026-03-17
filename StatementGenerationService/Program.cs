using Amazon.DynamoDBv2;
using StatementGenerationService;
using StatementGenerationService.Services;
using StatementGenerationService.Services.Interfaces;
using StatementGenerationService.Repositories;
using StatementGenerationService.Repositories.Interfaces;

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
