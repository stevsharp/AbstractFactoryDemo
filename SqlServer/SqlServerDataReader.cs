using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.SqlServer;

/// Concrete Product C (SQL Server) — simulates a SQL Server result set.
public class SqlServerDataReader : IDbDataReader
{
    private int _rowIndex = -1;

    private static readonly List<Dictionary<string, object>> Rows =
    [
        new() { ["TaskItemId"] = 1, ["TaskName"] = "Setup SQL Server", ["IsComplete"] = false },
        new() { ["TaskItemId"] = 2, ["TaskName"] = "Write stored procedures", ["IsComplete"] = true },
        new() { ["TaskItemId"] = 3, ["TaskName"] = "Configure backups", ["IsComplete"] = false },
    ];

    public bool Read()
    {
        _rowIndex++;
        return _rowIndex < Rows.Count;
    }

    public object GetValue(string columnName)
    {
        if (!Rows[_rowIndex].TryGetValue(columnName, out var value))
            throw new InvalidOperationException($"Column '{columnName}' not found.");
        return value;
    }

    public void Close()
    {
        _rowIndex = -1;
        Console.WriteLine("  [SQL Server] Reader closed.");
    }

    public Task<bool> ReadAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Read());
    }

    public Task CloseAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Close();
        return Task.CompletedTask;
    }
}
