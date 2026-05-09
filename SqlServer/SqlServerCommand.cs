using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.SqlServer;

/// Concrete Product B (SQL Server) — simulates a SQL Server command.
public class SqlServerCommand(string sql, SqlServerConnection connection) : IDbCommand
{
    private readonly SqlServerConnection _connection = connection;
    private readonly Dictionary<string, object> _parameters = new();

    public string CommandText { get; } = sql;

    public void AddParameter(string name, object value)
    {
        _parameters[name] = value;
        Console.WriteLine($"  [SQL Server] Parameter added: {name} = {value}");
    }

    public IDbDataReader ExecuteReader()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [SQL Server] Executing query: {CommandText}");
        return new SqlServerDataReader();
    }

    public int ExecuteNonQuery()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [SQL Server] Executing non-query: {CommandText}");
        return 1;
    }

    public object? ExecuteScalar()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [SQL Server] Executing scalar: {CommandText}");
        return 42;
    }

    public Task<IDbDataReader> ExecuteReaderAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(ExecuteReader());
    }

    public Task<int> ExecuteNonQueryAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(ExecuteNonQuery());
    }

    public Task<object?> ExecuteScalarAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(ExecuteScalar());
    }
}
