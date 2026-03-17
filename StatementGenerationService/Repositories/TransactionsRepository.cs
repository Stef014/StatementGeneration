using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using StatementGenerationService.Models;
using StatementGenerationService.Models.Enums;
using StatementGenerationService.Repositories.Interfaces;

namespace StatementGenerationService.Repositories;

public class TransactionsRepository : ITransactionsRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private const string TableName = "Transactions";

    public TransactionsRepository(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId, long startTimestamp, long endTimestamp, CancellationToken cancellationToken)
    {
        var request = new QueryRequest
        {
            TableName = TableName,
            KeyConditionExpression = "AccountId = :accountId AND TransactionTimestamp BETWEEN :startTimestamp AND :endTimestamp",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":accountId", new AttributeValue { S = accountId.ToString() } },
                { ":startTimestamp", new AttributeValue { N = startTimestamp.ToString() } },
                { ":endTimestamp", new AttributeValue { N = endTimestamp.ToString() } }
            }
        };

        var response = await _dynamoDbClient.QueryAsync(request, cancellationToken);
        return response.Items.Select(item => new Transaction
        {
            TransactionId = Guid.Parse(item["TransactionId"].S),
            TransactionTimestamp = long.Parse(item["TransactionTimestamp"].N),
            AccountId = Guid.Parse(item["AccountId"].S),
            Description = item["Description"].S,
            Category = int.Parse(item["Category"].N),
            Direction = int.Parse(item["TransactionDirection"].N),
            Amount = long.Parse(item["Amount"].N),
        });
    }
}