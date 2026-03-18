using StatementGenerationService.Models;

namespace StatementGenerationService.Services.Interfaces;

public interface IStatementsService
{
    Task<string> GenerateStatementAsync(StatementGenerationRequest request, string downloadUrl, CancellationToken cancellationToken);
}