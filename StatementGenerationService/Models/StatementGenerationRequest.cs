namespace StatementGenerationService.Models;

public class StatementGenerationRequest
{
    public required Guid AccountId { get; set; }
    public required string AccountHolderName { get; set; }
    public required long AccountBalance { get; set; }
    public required string AccountHolderEmailAddress { get; set; }
    public required long StartTimestamp { get; set; }
    public required long EndTimestamp { get; set; }
}