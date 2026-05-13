using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Oracle;

/// Concrete Product A (Oracle) — simulates an Oracle connection.
public class OracleConnection : IDbConnection
{
    private bool _isOpen;

    public OracleConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }
    public bool   IsOpen           => _isOpen;

    public void Open()
    {
        _isOpen = true;
        Console.WriteLine($"  [Oracle] Opening connection to: {ConnectionString}");
    }

    public void Close()
    {
        _isOpen = false;
        Console.WriteLine("  [Oracle] Connection closed.");
    }

    public IDbTransaction BeginTransaction()
    {
        if (!_isOpen) throw new InvalidOperationException("Connection is not open.");
        Console.WriteLine("  [Oracle] Transaction started.");
        return new OracleTransaction();
    }

    public Task OpenAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Open();
        return Task.CompletedTask;
    }

    public Task CloseAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Close();
        return Task.CompletedTask;
    }

    public Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(BeginTransaction());
    }
}
