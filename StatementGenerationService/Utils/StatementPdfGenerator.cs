
using System.ComponentModel;
using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StatementGenerationService.Models;
using StatementGenerationService.Models.Enums;

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

    private static string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
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
                    columns.RelativeColumn(15);
                    columns.RelativeColumn(30);
                    columns.RelativeColumn(25);
                    columns.RelativeColumn(15);
                    columns.RelativeColumn(15);
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
                    table.Cell().Text(DateTimeOffset.FromUnixTimeMilliseconds(transaction.TransactionTimestamp).ToString("yyyy-MM-dd"));
                    table.Cell().Text(transaction.Description);
                    table.Cell().Text(GetEnumDescription((TransactionCategories)transaction.Category));
                    if (transaction.Direction == 1) // Money In
                    {
                        table.Cell().Text($"R{transaction.Amount / 100m:F2}");
                        table.Cell().Text("");
                    }
                    else // Money Out
                    {
                        table.Cell().Text("");
                        table.Cell().Text($"R{transaction.Amount / 100m:F2}");
                    }
                }
            });
        });
    }

}