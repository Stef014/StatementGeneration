namespace StatementGenerationService.Utils;

public class StatementLinkMailGenerator
{
    public static string GenerateStatementEmailBody(string accountHolderName, string downloadUrl)
    {
        return $@"
            <html>
                <body>
                    <p>Dear {accountHolderName},</p>
                    <br/> <br/> 
                    <p>Your monthly statement is available for download.</p> <br/>
                    <p>Please use the following link to download the statement:</p> <br/>
                    <p><a href='{downloadUrl}'>Download Statement</a></p>
                    <p>This link will expire in 24 hours.</p> <br/>
                    <p>Best regards,<br/>Capitec Bank</p>
                </body>
            </html>";
    }
}