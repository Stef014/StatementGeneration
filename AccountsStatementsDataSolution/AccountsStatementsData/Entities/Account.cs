namespace AccountsStatementsData.Entities;

public class Account
{
    public required Guid AccountId { get; set; }
    public required string AccountHolderName { get; set; }
    public required string AccountHolderEmailAddress { get; set; }
    public long ClosingBalance { get; set; }

    public ICollection<Statement> Statements { get; set; } = new List<Statement>();
}
