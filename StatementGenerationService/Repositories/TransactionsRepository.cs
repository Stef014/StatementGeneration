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
        Console.WriteLine($"Querying transactions for AccountId: {accountId}, StartTimestamp: {startTimestamp}, EndTimestamp: {endTimestamp}");

        var request = new QueryRequest
        {
            TableName = TableName,
            KeyConditionExpression = "AccountID = :accountId AND TransactionTimestamp BETWEEN :startTimestamp AND :endTimestamp",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":accountId", new AttributeValue { S = accountId.ToString() } },
                { ":startTimestamp", new AttributeValue { N = startTimestamp.ToString() } },
                { ":endTimestamp", new AttributeValue { N = endTimestamp.ToString() } }
            }
        };

        var response = await _dynamoDbClient.QueryAsync(request, cancellationToken);
        
        Console.WriteLine($"Query executed. Items retrieved: {response.Items.Count}");

        return response.Items.Select(item => new Transaction
        {
            TransactionTimestamp = long.Parse(item["TransactionTimestamp"].N),
            AccountId = Guid.Parse(item["AccountID"].S),
            Description = item["Description"].S,
            Category = int.Parse(item["Category"].N),
            Direction = int.Parse(item["Direction"].N),
            Amount = long.Parse(item["Amount"].N),
        });
    }
}