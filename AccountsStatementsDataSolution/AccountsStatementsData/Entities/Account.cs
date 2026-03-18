namespace AccountsStatementsData.Entities;

public class Account
{
    public Guid AccountId { get; set; }
    public string AccountHolderName { get; set; } = string.Empty;
    public long ClosingBalance { get; set; }

    public ICollection<Statement> Statements { get; set; } = new List<Statement>();
}
