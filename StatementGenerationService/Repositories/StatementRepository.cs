using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using StatementGenerationService.Models;
using StatementGenerationService.Repositories.Interfaces;

namespace StatementGenerationService.Repositories;

public class StatementRepository : IStatementRepository
{
    private readonly string _connectionString;

    public StatementRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AccountsStatementsConnection");
    }

    public async Task SaveStatementAsync(Statement statement, CancellationToken cancellationToken)
    {
        statement.StatementId = Guid.NewGuid();

        var insertQuery = "INSERT INTO \"Statements\" (\"StatementId\", \"AccountId\", \"StartTimestamp\", \"EndTimestamp\", \"DownloadUrl\") VALUES (@StatementId, @AccountId, @StartTimestamp, @EndTimestamp, @DownloadUrl)";
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(insertQuery, statement);
    }
}