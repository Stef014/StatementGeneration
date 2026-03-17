using StatementGenerationService.Models;

namespace StatementGenerationService.Repositories.Interfaces;

public interface ITransactionsRepository
{
    Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(Guid accountId, long startTimestamp, long endTimestamp, CancellationToken cancellationToken);
}