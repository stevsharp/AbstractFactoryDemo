using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.SqlServer;

/// Concrete Product B (SQL Server) — simulates a SQL Server command.
public class SqlServerCommand(string sql, SqlServerConnection connection) : IDbCommand
{
    private readonly SqlServerConnection _connection  = connection;
    private readonly Dictionary<string, IDbParameter> _parameters = new();
    private IDbTransaction? _transaction;

    public string CommandText { get; } = sql;

    public void AddParameter(string name, object? value)
    {
        var p = new SqlServerParameter { Name = name, Value = value };
        _parameters[name] = p;
        Console.WriteLine($"  [SQL Server] Param: {name} = {value}");
    }

    public void AddParameter(IDbParameter parameter)
    {
        _parameters[parameter.Name] = parameter;
        string sizeHint = parameter.Size > 0 ? $", Size={parameter.Size}" : string.Empty;
        Console.WriteLine($"  [SQL Server] Param: {parameter.Name} = {parameter.Value ?? "null"} " +
                          $"[{parameter.Direction}, {parameter.DbType}{sizeHint}]");
    }

    public IDbParameter? GetParameter(string name) =>
        _parameters.TryGetValue(name, out var p) ? p : null;

    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
        Console.WriteLine("  [SQL Server] Command enlisted in transaction.");
    }

    public IDbDataReader ExecuteReader()
    {
        EnsureOpen();
        Console.WriteLine($"  [SQL Server] Executing reader: {CommandText}");
        return new SqlServerDataReader();
    }

    public int ExecuteNonQuery()
    {
        EnsureOpen();
        Console.WriteLine($"  [SQL Server] Executing non-query: {CommandText}" +
                          (_transaction is not null ? " [in transaction]" : string.Empty));
        PopulateOutputParameters();
        return 1;
    }

    public object? ExecuteScalar()
    {
        EnsureOpen();
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

    private void EnsureOpen()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");
    }

    private void PopulateOutputParameters()
    {
        foreach (var p in _parameters.Values)
        {
            if (p is SqlServerParameter sp)
            {
                sp.OutputValue = p.Direction switch
                {
                    DbParameterDirection.ReturnValue => 0,
                    DbParameterDirection.Output or DbParameterDirection.InputOutput =>
                        SimulateValue(p.DbType),
                    _ => sp.OutputValue
                };
            }
        }
    }

    private static object SimulateValue(DbType dbType) => dbType switch
    {
        DbType.Int16 or DbType.Int32 or DbType.Int64 => 42,
        DbType.Decimal or DbType.Double              => 99.99m,
        DbType.Boolean                               => true,
        DbType.DateTime or DbType.Date               => DateTime.UtcNow,
        DbType.Guid                                  => Guid.NewGuid(),
        DbType.String                                => "output-value",
        _                                            => DBNull.Value
    };
}
