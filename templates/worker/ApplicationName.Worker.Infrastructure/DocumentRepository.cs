using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using ApplicationName.Shared.Projections;
using ApplicationName.Worker.Application;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts;
using ArgDefender;
using MapsterMapper;
using Npgsql;
using NpgsqlTypes;

namespace ApplicationName.Worker.Infrastructure;

[ExcludeFromCodeCoverage] // thin glue: translator + Npgsql + STJ; behaviour covered end-to-end
public class DocumentRepository(
    NpgsqlDataSource dataSource,
    IMapper mapper,
    IProtoCacheRepository protoCacheRepository) : IDocumentRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(); // PascalCase, case-sensitive (NOT Web)

    private static string GetTableName(Type t) => $"{t.Name.ToLowerInvariant()}s"; // ExampleDocument -> exampledocuments

    // runs "SELECT data FROM {table} WHERE {where}" (where/parms optional) and deserializes each row
    private async Task<List<string>> QueryDataAsync(string table, string? where, NpgsqlParameter[]? parms)
    {
        var sql = where is null ? $"SELECT data FROM {table}" : $"SELECT data FROM {table} WHERE {where}";
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        if (parms is not null) command.Parameters.AddRange(parms);

        var rows = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) rows.Add(reader.GetString(0));
        return rows;
    }

    public async Task<T> GetAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase
    {
        Guard.Argument(expr).NotNull();
        var (where, parms) = JsonbPredicateTranslator.Translate(expr);
        var rows = await QueryDataAsync(GetTableName(typeof(T)), where, parms);
        if (rows.Count > 1) throw new InvalidOperationException("Sequence contains more than one element"); // mirror SingleOrDefault
        return rows.Count == 0 ? null! : JsonSerializer.Deserialize<T>(rows[0], JsonOptions)!;
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : DocumentBase
    {
        var rows = await QueryDataAsync(GetTableName(typeof(T)), null, null);
        return rows.Select(j => JsonSerializer.Deserialize<T>(j, JsonOptions)!);
    }

    public async Task<IEnumerable<DocumentBase>> GetAllByTypeAsync(Type documentType)
    {
        Guard.Argument(documentType).NotNull();
        var rows = await QueryDataAsync(GetTableName(documentType), null, null);
        return rows.Select(j => (DocumentBase)JsonSerializer.Deserialize(j, documentType, JsonOptions)!);
    }

    public async Task<DocumentBase?> GetByIdAndTypeAsync(Guid id, Type documentType)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(documentType).NotNull();
        var rows = await QueryDataAsync(GetTableName(documentType), "id = @id", [new NpgsqlParameter("id", id)]);
        return rows.Count == 0 ? null : (DocumentBase)JsonSerializer.Deserialize(rows[0], documentType, JsonOptions)!;
    }

    public async Task UpsertAsync(ExampleDocument document)
    {
        Guard.Argument(document).NotNull();
        Guard.Argument(document.Id).NotDefault();
        Guard.Argument(document.Created).NotDefault();
        Guard.Argument(document.Updated).NotDefault();

        var table = GetTableName(typeof(ExampleDocument));
        var json = JsonSerializer.Serialize(document, JsonOptions);

        // ON CONFLICT (id) leaves created untouched -> Mongo SetOnInsert(Created)/Set(Updated) semantics.
        var sql = $"""
            INSERT INTO {table} (id, created, updated, data)
            VALUES (@id, @created, @updated, @data)
            ON CONFLICT (id) DO UPDATE SET updated = EXCLUDED.updated, data = EXCLUDED.data;
            """;

        await using (var connection = await dataSource.OpenConnectionAsync())
        await using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("id", document.Id);
            command.Parameters.AddWithValue("created", document.Created);
            command.Parameters.AddWithValue("updated", document.Updated);
            command.Parameters.Add(new NpgsqlParameter("data", NpgsqlDbType.Jsonb) { Value = json });
            await command.ExecuteNonQueryAsync();
        }

        var projection = mapper.Map<ExampleProjection>(document);
        await protoCacheRepository.SetAsync(ApplicationConstants.ExampleProjectionByIdCacheKey(document.Id), projection);
    }
}
