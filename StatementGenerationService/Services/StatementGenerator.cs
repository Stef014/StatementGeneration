using StatementGenerationService.Models.Enums;
using StatementGenerationService.Repositories.Interfaces;
using StatementGenerationService.Services.Interfaces;

namespace StatementGenerationService.Services;

public class StatementGenerator : IReportGenerator
{
    private readonly ITransactionsRepository _transactionsRepository;

    public StatementGenerator(ITransactionsRepository transactionsRepository)
    {
        _transactionsRepository = transactionsRepository;

    }
 
    public async Task GenerateReportAsync(Guid accountId, long startTimestamp, long endTimestamp, CancellationToken cancellationToken)
    {
        var transactionsForInvoice = await _transactionsRepository.GetTransactionsByAccountIdAsync(accountId, startTimestamp, endTimestamp, cancellationToken);
    }
}