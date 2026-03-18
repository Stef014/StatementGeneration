
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StatementGenerationService.Models;

namespace StatementGenerationService.Utils;

public class StatementPdfGenerator : IDocument
{
    private readonly Guid _accountId;
    private readonly string _accountHolderName;
    private readonly IEnumerable<Transaction> _transactions;    
    
    public StatementPdfGenerator(Guid accountId, string accountHolderName, IEnumerable<Transaction> transactions)
    {
        _accountId = accountId;
        _accountHolderName = accountHolderName;
        _transactions = transactions;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(50);
            page.Header().Text($"Statement for Account: {_accountId} -  Holder: {_accountHolderName}");
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(20);
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(10);
                    columns.RelativeColumn(10);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Date");
                    header.Cell().Text("Description");
                    header.Cell().Text("Category");
                    header.Cell().Text("Money In");
                    header.Cell().Text("Money Out");
                });

                foreach (var transaction in _transactions)
                {
                    table.Cell().Text(transaction.TransactionTimestamp.ToString());
                    table.Cell().Text(transaction.Description);
                    table.Cell().Text(transaction.Category.ToString());
                    if (transaction.Direction == 1) // Money In
                    {
                        table.Cell().Text(transaction.Amount.ToString());
                        table.Cell().Text("");
                    }
                    else // Money Out
                    {
                        table.Cell().Text("");
                        table.Cell().Text(transaction.Amount.ToString());
                    }
                }
            });
        });
    }

}