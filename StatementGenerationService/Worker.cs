using StatementGenerationService.Services.Interfaces;
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
                    var reportGenerator = scope.ServiceProvider.GetRequiredService<IReportGenerator>();
                    var fileName = await reportGenerator.GenerateReportAsync(request.AccountId, request.AccountHolderName, request.StartTimestamp, request.EndTimestamp, stoppingToken);
                    _logger.LogInformation("Statement generated: {fileName}", fileName);

                    var fileManagementService = scope.ServiceProvider.GetRequiredService<IFileManagementService>();
                    var uploadedStatementUrl = await fileManagementService.UploadFileAsync(fileName, stoppingToken);
                    _logger.LogInformation("Statement uploaded to storage: {uploadedStatementUrl}", uploadedStatementUrl);

                    var statementsService = scope.ServiceProvider.GetRequiredService<IStatementsService>();
                    await statementsService.GenerateStatementAsync(request, uploadedStatementUrl, stoppingToken);
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
}
