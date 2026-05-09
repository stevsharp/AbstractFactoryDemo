using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.MySql;

/// Concrete Product B (MySQL) — simulates a MySQL command.
public class MySqlCommand : IDbCommand
{
    private readonly MySqlConnection _connection;
    private readonly Dictionary<string, object> _parameters = new();

    public MySqlCommand(string sql, MySqlConnection connection)
    {
        CommandText = sql;
        _connection = connection;
    }

    public string CommandText { get; }

    public void AddParameter(string name, object value)
    {
        _parameters[name] = value;
        Console.WriteLine($"  [MySQL] Parameter added: {name} = {value}");
    }

    public IDbDataReader ExecuteReader()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [MySQL] Executing query: {CommandText}");
        return new MySqlDataReader();
    }

    public int ExecuteNonQuery()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [MySQL] Executing non-query: {CommandText}");
        return 1;
    }

    public object? ExecuteScalar()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");

        Console.WriteLine($"  [MySQL] Executing scalar: {CommandText}");
        return 99;
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
