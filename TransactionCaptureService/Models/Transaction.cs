using TransactionCaptureService.Models.Enums;

namespace TransactionCaptureService.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public string Description { get; set; }
    public TransactionCategories Category { get; set; }
    public TransactionDirections Direction { get; set; }
}