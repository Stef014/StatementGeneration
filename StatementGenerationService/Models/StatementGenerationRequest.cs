namespace StatementGenerationService.Models;

public class StatementGenerationRequest
{
    public Guid AccountId { get; set; }
    public string AccountHolderName { get; set; }
    public long StartTimestamp { get; set; }
    public long EndTimestamp { get; set; }
}