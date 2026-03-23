using System.IO.Enumeration;
using QuestPDF.Fluent;
using StatementGenerationService.Models.Enums;
using StatementGenerationService.Repositories.Interfaces;
using StatementGenerationService.Services.Interfaces;
using StatementGenerationService.Utils;

namespace StatementGenerationService.Services;

public class StatementGenerator : IReportGenerator
{
    private readonly ITransactionsRepository _transactionsRepository;

    public StatementGenerator(ITransactionsRepository transactionsRepository)
    {
        _transactionsRepository = transactionsRepository;

    }
 
    public async Task<string> GenerateReportAsync(Guid accountId, string accountHolderName, long accountBalance, long startTimestamp, long endTimestamp, CancellationToken cancellationToken)
    {
        var fileName = $"./Statements/{accountId}_{DateTime.Now:yyyyMMdd}_Statement.pdf";
        
        var transactionsForInvoice = await _transactionsRepository.GetTransactionsByAccountIdAsync(accountId, startTimestamp, endTimestamp, cancellationToken);
        
        var pdfGenerator = new StatementPdfGenerator(accountId, accountHolderName, accountBalance, transactionsForInvoice);
        pdfGenerator.GeneratePdf(fileName);

        return fileName;
    }
}