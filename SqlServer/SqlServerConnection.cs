using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.SqlServer;

/// Concrete Product A (SQL Server) — simulates a SQL Server connection.
public class SqlServerConnection : IDbConnection
{
    private bool _isOpen;

    public SqlServerConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public bool IsOpen => _isOpen;

    public void Open()
    {
        _isOpen = true;
        Console.WriteLine($"  [SQL Server] Opening connection to: {ConnectionString}");
    }

    public void Close()
    {
        _isOpen = false;
        Console.WriteLine("  [SQL Server] Connection closed.");
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
}
