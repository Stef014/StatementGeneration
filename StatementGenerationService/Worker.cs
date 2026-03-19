using StatementGenerationService.Services.Interfaces;
using StatementGenerationService.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace StatementGenerationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: This will arrive from a queue
        var request = new Models.StatementGenerationRequest
        {
            AccountId = Guid.Parse("9d209565-ce8f-4a0d-bb73-e8fec2bbcd08"),
            AccountHolderName = "John Doe",
            AccountHolderEmailAddress = "john.doe@example.com",
            StartTimestamp = DateTimeOffset.Now.AddDays(-90).ToUnixTimeMilliseconds(),
            EndTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var fileName = await GenerateStatement(scope.ServiceProvider, request, stoppingToken);

                    var uploadedStatementUrl = await UploadStatementToStorage(scope.ServiceProvider, fileName, stoppingToken);

                    await SaveAccountStatement(scope.ServiceProvider, request, uploadedStatementUrl, stoppingToken);

                    await SendStatementMail(scope.ServiceProvider, request, uploadedStatementUrl);
                }

                _logger.LogInformation("Statement generation completed at: {time}", DateTimeOffset.Now);    
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the statement.");
                continue;
            }
            

            await Task.Delay(1000, stoppingToken);
        }
    }

    private static async Task SaveAccountStatement(IServiceProvider serviceProvider, Models.StatementGenerationRequest request, string uploadedStatementUrl, CancellationToken cancellationToken)
    {
        var statementsService = serviceProvider.GetRequiredService<IStatementsService>();
        await statementsService.GenerateStatementAsync(request, uploadedStatementUrl, cancellationToken);
    }

    private async Task<string> GenerateStatement(IServiceProvider serviceProvider, Models.StatementGenerationRequest request, CancellationToken cancellationToken)
    {
        var reportGenerator = serviceProvider.GetRequiredService<IReportGenerator>();
        var fileName = await reportGenerator.GenerateReportAsync(request.AccountId, request.AccountHolderName, request.StartTimestamp, request.EndTimestamp, cancellationToken);
        _logger.LogInformation("Statement generated: {fileName}", fileName);
        return fileName;
    }

    private async Task<string> UploadStatementToStorage(IServiceProvider serviceProvider, string fileName, CancellationToken cancellationToken)
    {
        var fileManagementService = serviceProvider.GetRequiredService<IFileManagementService>();
        var uploadedStatementUrl = await fileManagementService.UploadFileAsync(fileName, cancellationToken);
        _logger.LogInformation("Statement uploaded to storage: {uploadedStatementUrl}", uploadedStatementUrl);
        return uploadedStatementUrl;
    }

    private static async Task SendStatementMail(IServiceProvider serviceProvider, Models.StatementGenerationRequest request, string uploadedStatementUrl)
    {
        var mailingService = serviceProvider.GetRequiredService<IMailingService>();
        var emailBody = StatementLinkMailGenerator.GenerateStatementEmailBody(request.AccountHolderName, uploadedStatementUrl);
        await mailingService.SendEmailAsync(request.AccountHolderEmailAddress, $"Monthly Statement for Account: {request.AccountId}", emailBody);
    }
}
