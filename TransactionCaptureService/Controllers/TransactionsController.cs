using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TransactionCaptureService.Configuration;
using TransactionCaptureService.Services.Interfaces;

namespace TransactionCaptureService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ILogger<TransactionsController> _logger;
    private readonly ApiSettings _apiSettings;
    private readonly ITransactionCaptureService _transactionService;

    public TransactionsController(
        ILogger<TransactionsController> logger,
        IOptions<ApiSettings> apiSettings,
        ITransactionCaptureService transactionService)
    {
        _logger = logger;
        _apiSettings = apiSettings.Value;
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Transaction transaction, [FromHeader(Name = "X-API-Key")] string apiKey)
    {
        if (apiKey != _apiSettings.ApiKey)
        {
            return Unauthorized();
        }

        await _transactionService.CaptureTransactionAsync(transaction);
        
        return Accepted();
    }
}
