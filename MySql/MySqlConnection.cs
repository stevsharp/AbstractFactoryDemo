using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.MySql;

/// Concrete Product A (MySQL) — simulates a MySQL connection.
public class MySqlConnection : IDbConnection
{
    private bool _isOpen;

    public MySqlConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }
    public bool   IsOpen           => _isOpen;

    public void Open()
    {
        _isOpen = true;
        Console.WriteLine($"  [MySQL] Opening connection to: {ConnectionString}");
    }

    public void Close()
    {
        _isOpen = false;
        Console.WriteLine("  [MySQL] Connection closed.");
    }

    public IDbTransaction BeginTransaction()
    {
        if (!_isOpen) throw new InvalidOperationException("Connection is not open.");
        Console.WriteLine("  [MySQL] Transaction started.");
        return new MySqlTransaction();
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
