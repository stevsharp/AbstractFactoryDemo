using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Oracle;

/// Concrete Product B (Oracle) — simulates an Oracle command.
public class OracleCommand(string sql, OracleConnection connection) : IDbCommand
{
    private readonly OracleConnection _connection = connection;
    private readonly Dictionary<string, object> _parameters = new();

    public string CommandText { get; } = sql;

    public void AddParameter(string name, object value)
    {
        _parameters[name] = value;
        Console.WriteLine($"  [Oracle] Parameter added: {name} = {value}");
    }

    public IDbDataReader ExecuteReader()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [Oracle] Executing query: {CommandText}");
        return new OracleDataReader();
    }

    public int ExecuteNonQuery()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [Oracle] Executing non-query: {CommandText}");
        return 1;
    }

    public object? ExecuteScalar()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [Oracle] Executing scalar: {CommandText}");
        return 77;
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
