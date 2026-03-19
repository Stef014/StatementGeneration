using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StatementGenerationService.Jobs.Interfaces;
using StatementGenerationService.Models;
using StatementGenerationService.Services.Interfaces;
using StatementGenerationService.Utils;

namespace StatementGenerationService.Jobs;

public class StatementGenerationJob : IJob<StatementGenerationRequest>
{
    private readonly ILogger<StatementGenerationJob> _logger;
    
    private readonly IFileManagementService _fileManagementService;
    private readonly IStatementsService _statementsService;
    private readonly IReportGenerator _reportGenerator;
    private readonly IMailingService _mailingService;
    
    public StatementGenerationJob(
        ILogger<StatementGenerationJob> logger,
        IFileManagementService fileManagementService,
        IStatementsService statementsService,
        IReportGenerator reportGenerator,
        IMailingService mailingService)
    {
        _logger = logger;
        _fileManagementService = fileManagementService;
        _statementsService = statementsService;
        _reportGenerator = reportGenerator;
        _mailingService = mailingService;
     }
    


    public async Task ExecuteAsync(StatementGenerationRequest input, CancellationToken cancellationToken)
    {
        var fileName = await GenerateStatement(input, cancellationToken);

        var uploadedStatementUrl = await UploadStatementToStorage(input, fileName, cancellationToken);

        await SaveAccountStatement(input, uploadedStatementUrl, cancellationToken);

        await SendStatementMail(input, uploadedStatementUrl);
    }

    private async Task<string> GenerateStatement(StatementGenerationRequest request, CancellationToken cancellationToken)
    {
        var fileName = await _reportGenerator.GenerateReportAsync(request.AccountId, request.AccountHolderName, request.StartTimestamp, request.EndTimestamp, cancellationToken);
        _logger.LogInformation("Statement generated: {fileName}", fileName);
        return fileName;
    }

    private async Task<string> UploadStatementToStorage(StatementGenerationRequest request, string fileName, CancellationToken cancellationToken)
    {
        var fileManagementService = _fileManagementService;
        var uploadedStatementUrl = await fileManagementService.UploadFileAsync(fileName, cancellationToken);
        _logger.LogInformation("Statement uploaded to storage: {uploadedStatementUrl}", uploadedStatementUrl);
        return uploadedStatementUrl;
    }
    private async Task SendStatementMail(StatementGenerationRequest request, string uploadedStatementUrl)
    {
        var mailingService = _mailingService;
        var emailBody = StatementLinkMailGenerator.GenerateStatementEmailBody(request.AccountHolderName, uploadedStatementUrl);
        await mailingService.SendEmailAsync(request.AccountHolderEmailAddress, $"Monthly Statement for Account: {request.AccountId}", emailBody);
    }

    private async Task SaveAccountStatement(StatementGenerationRequest request, string uploadedStatementUrl, CancellationToken cancellationToken)
    {
        var statementsService = _statementsService;
        await statementsService.GenerateStatementAsync(request, uploadedStatementUrl, cancellationToken);
    }
}