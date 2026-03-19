using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using StatementGenerationService.Services.Interfaces;

namespace StatementGenerationService.Services;

public class MailingService : IMailingService
{
    private readonly IAmazonSimpleEmailService _sesClient;

    public MailingService(IAmazonSimpleEmailService sesClient)
    {
        _sesClient = sesClient;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var sendRequest = new SendEmailRequest
        {
            Source = "noreply@capitecbank.co.za",
            Destination = new Destination
            {
                ToAddresses = new List<string> { toEmail }
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = body
                    }
                }
            }
        };

        await _sesClient.SendEmailAsync(sendRequest);
    }
}