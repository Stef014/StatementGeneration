namespace StatementGenerationService.Services.Interfaces;

public interface IMailingService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}