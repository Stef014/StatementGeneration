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
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var reportGenerator = scope.ServiceProvider.GetRequiredService<IReportGenerator>();
                    var fileName = await reportGenerator.GenerateReportAsync(Guid.Parse("9d209565-ce8f-4a0d-bb73-e8fec2bbcd08"), DateTimeOffset.Now.AddDays(-90).ToUnixTimeMilliseconds(), DateTimeOffset.Now.ToUnixTimeMilliseconds(), stoppingToken);
                    _logger.LogInformation("Report generated: {fileName}", fileName);

                    var fileManagementService = scope.ServiceProvider.GetRequiredService<IFileManagementService>();
                    var uploadedStatementUrl = await fileManagementService.UploadFileAsync(fileName, stoppingToken);
                    _logger.LogInformation("Report uploaded to storage: {uploadedStatementUrl}", uploadedStatementUrl);
                }

                _logger.LogInformation("Report generation completed at: {time}", DateTimeOffset.Now);    
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the report.");
                continue;
            }
            

            await Task.Delay(1000, stoppingToken);
        }
    }
}
