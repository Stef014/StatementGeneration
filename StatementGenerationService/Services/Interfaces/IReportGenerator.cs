using StatementGenerationService.Models.Enums;

namespace StatementGenerationService.Services.Interfaces;

public interface IReportGenerator
{
    Task<string> GenerateReportAsync(Guid accountId, string accountHolderName, long accountBalance, long startTimestamp, long endTimestamp, CancellationToken cancellationToken);
}