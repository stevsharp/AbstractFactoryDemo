using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Oracle;

/// Concrete Product B (Oracle) — simulates an Oracle command.
public class OracleCommand(string sql, OracleConnection connection) : IDbCommand
{
    private readonly OracleConnection _connection = connection;
    private readonly Dictionary<string, IDbParameter> _parameters = new();
    private IDbTransaction? _transaction;

    public string CommandText { get; } = sql;

    public void AddParameter(string name, object? value)
    {
        var p = new OracleParameter { Name = name, Value = value };
        _parameters[name] = p;
        Console.WriteLine($"  [Oracle] Param: {name} = {value}");
    }

    public void AddParameter(IDbParameter parameter)
    {
        _parameters[parameter.Name] = parameter;
        string sizeHint = parameter.Size > 0 ? $", Size={parameter.Size}" : string.Empty;
        Console.WriteLine($"  [Oracle] Param: {parameter.Name} = {parameter.Value ?? "null"} " +
                          $"[{parameter.Direction}, {parameter.DbType}{sizeHint}]");
    }

    public IDbParameter? GetParameter(string name) =>
        _parameters.TryGetValue(name, out var p) ? p : null;

    public void SetTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
        Console.WriteLine("  [Oracle] Command enlisted in transaction.");
    }

    public IDbDataReader ExecuteReader()
    {
        EnsureOpen();
        Console.WriteLine($"  [Oracle] Executing reader: {CommandText}");
        return new OracleDataReader();
    }

    public int ExecuteNonQuery()
    {
        EnsureOpen();
        Console.WriteLine($"  [Oracle] Executing non-query: {CommandText}" +
                          (_transaction is not null ? " [in transaction]" : string.Empty));
        PopulateOutputParameters();
        return 1;
    }

    public object? ExecuteScalar()
    {
        EnsureOpen();
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

    private void EnsureOpen()
    {
        if (!_connection.IsOpen)
            throw new InvalidOperationException("Connection is not open.");
    }

    private void PopulateOutputParameters()
    {
        foreach (var p in _parameters.Values)
        {
            if (p is OracleParameter op)
            {
                op.OutputValue = p.Direction switch
                {
                    DbParameterDirection.ReturnValue => 0,
                    DbParameterDirection.Output or DbParameterDirection.InputOutput =>
                        SimulateValue(p.DbType),
                    _ => op.OutputValue
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
