using StatementGenerationService.Models;
using StatementGenerationService.Repositories.Interfaces;
using StatementGenerationService.Services.Interfaces;

namespace StatementGenerationService.Services;

public class StatementsService : IStatementsService
{
    private readonly IStatementRepository _statementRepository;

    public StatementsService(IStatementRepository statementRepository)
    {
        _statementRepository = statementRepository;
    }

    public async Task<string> GenerateStatementAsync(StatementGenerationRequest request, string downloadUrl, CancellationToken cancellationToken)
    {
        // Save the statement metadata to the repository
        var statement = new Statement
        {
            AccountId = request.AccountId,
            StartTimestamp = request.StartTimestamp,
            EndTimestamp = request.EndTimestamp,
            DownloadUrl = downloadUrl
        };
        await _statementRepository.SaveStatementAsync(statement, cancellationToken);

        return downloadUrl;
    }
}
