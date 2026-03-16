using System.Transactions;

namespace TransactionCaptureService.Services.Interfaces;

public interface ITransactionCaptureService
{
    public Task CaptureTransactionAsync(Transaction transaction);
}
