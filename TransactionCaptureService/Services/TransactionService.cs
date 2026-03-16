using TransactionCaptureService.Models;
using TransactionCaptureService.Services.Interfaces;

namespace TransactionCaptureService.Services;

public class TransactionService : ITransactionCaptureService
{
    public async Task CaptureTransactionAsync(System.Transactions.Transaction transaction)
    {
        throw new NotImplementedException();
    }
}