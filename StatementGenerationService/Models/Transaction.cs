using Amazon.DynamoDBv2.DataModel;

namespace StatementGenerationService.Models;

[DynamoDBTable("Transactions")]
public class Transaction
{
    public Guid TransactionId { get; set; }

    [DynamoDBHashKey]
    public Guid AccountId { get; set; }
    
    [DynamoDBRangeKey]
    public long TransactionTimestamp { get; set; }        
    
    [DynamoDBProperty]
    public string Description { get; set; }

    [DynamoDBProperty]
    public int Category { get; set; }

    [DynamoDBProperty]
    public int Direction { get; set; }

    [DynamoDBProperty]
    public long Amount { get; set; }
}