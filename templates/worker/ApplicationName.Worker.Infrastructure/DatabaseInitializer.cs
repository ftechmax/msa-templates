using System.Diagnostics.CodeAnalysis;
using Npgsql;

namespace ApplicationName.Worker.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DatabaseInitializer
{
    private static string GetTableName(Type t) => $"{t.Name.ToLowerInvariant()}s";

    public static async Task InitializeAsync(NpgsqlDataSource dataSource, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        foreach (var mapping in CacheProjectionRegistry.Mappings)
        {
            var table = GetTableName(mapping.DocumentType);
            var sql = $"""
                CREATE TABLE IF NOT EXISTS {table} (
                  id uuid PRIMARY KEY,
                  created timestamptz NOT NULL,
                  updated timestamptz NOT NULL,
                  data jsonb NOT NULL);
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
