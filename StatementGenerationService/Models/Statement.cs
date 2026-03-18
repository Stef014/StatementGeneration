namespace StatementGenerationService.Models;

public class Statement 
{
    public Guid StatementId { get; set; }
    public Guid AccountId { get; set; }
    public long StartTimestamp { get; set; }
    public long EndTimestamp { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}