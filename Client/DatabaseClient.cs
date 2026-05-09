using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Client;

/// Client — depends only on IDatabaseFactory abstractions.
/// Never references SqlServer*, MySql*, or Oracle* types directly.
public class DatabaseClient(IDatabaseFactory factory)
{
    private readonly IDatabaseFactory _factory = factory;

    public async Task RunQueryAsync(string connectionString, string sql, int limit = 10,
        CancellationToken ct = default)
    {
        string limitedSql = _factory.Dialect.ApplyLimit(sql, limit);
        Console.WriteLine($"\n--- RunQueryAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {limitedSql}");

        IDbConnection conn = _factory.CreateConnection(connectionString);
        await conn.OpenAsync(ct);

        IDbCommand cmd = _factory.CreateCommand(limitedSql, conn);

        IDbDataReader reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            Console.WriteLine(
                $"  TaskItemId={reader.GetValue("TaskItemId"),-4} " +
                $"TaskName={reader.GetValue("TaskName"),-35} " +
                $"IsComplete={reader.GetValue("IsComplete")}");
        }
        await reader.CloseAsync(ct);

        await conn.CloseAsync(ct);
    }

    public async Task RunInsertAsync(string connectionString, string sql,
        string taskName, bool isComplete,
        CancellationToken ct = default)
    {
        Console.WriteLine($"\n--- RunInsertAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {sql}");

        IDbConnection conn = _factory.CreateConnection(connectionString);
        await conn.OpenAsync(ct);

        IDbCommand cmd = _factory.CreateCommand(sql, conn);
        cmd.AddParameter(_factory.Dialect.FormatParameter("TaskName"), taskName);
        cmd.AddParameter(_factory.Dialect.FormatParameter("IsComplete"), isComplete);

        int affected = await cmd.ExecuteNonQueryAsync(ct);
        Console.WriteLine($"  Rows affected: {affected}");

        await conn.CloseAsync(ct);
    }

    public async Task RunUpdateAsync(string connectionString, string sql,
        int taskItemId, bool isComplete,
        CancellationToken ct = default)
    {
        Console.WriteLine($"\n--- RunUpdateAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {sql}");

        IDbConnection conn = _factory.CreateConnection(connectionString);
        await conn.OpenAsync(ct);

        IDbCommand cmd = _factory.CreateCommand(sql, conn);
        cmd.AddParameter(_factory.Dialect.FormatParameter("IsComplete"), isComplete);
        cmd.AddParameter(_factory.Dialect.FormatParameter("TaskItemId"), taskItemId);

        int affected = await cmd.ExecuteNonQueryAsync(ct);
        Console.WriteLine($"  Rows affected: {affected}");

        await conn.CloseAsync(ct);
    }

    public async Task RunScalarAsync(string connectionString, string sql,
        CancellationToken ct = default)
    {
        Console.WriteLine($"\n--- RunScalarAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {sql}");

        IDbConnection conn = _factory.CreateConnection(connectionString);
        await conn.OpenAsync(ct);

        IDbCommand cmd = _factory.CreateCommand(sql, conn);

        object? result = await cmd.ExecuteScalarAsync(ct);
        Console.WriteLine($"  Scalar result: {result}");

        await conn.CloseAsync(ct);
    }
}
