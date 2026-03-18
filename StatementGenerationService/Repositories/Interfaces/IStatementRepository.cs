using StatementGenerationService.Models;

namespace StatementGenerationService.Repositories.Interfaces;

public interface IStatementRepository
{
    public Task SaveStatementAsync(Statement statement, CancellationToken cancellationToken);
}