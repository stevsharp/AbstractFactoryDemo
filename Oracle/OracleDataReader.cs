using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Oracle;

/// Concrete Product C (Oracle) — simulates an Oracle result set.
public class OracleDataReader : IDbDataReader
{
    private int _rowIndex = -1;

    private static readonly List<Dictionary<string, object>> Rows =
    [
        new() { ["TaskItemId"] = 1, ["TaskName"] = "Create Oracle tablespace", ["IsComplete"] = false },
        new() { ["TaskItemId"] = 2, ["TaskName"] = "Grant privileges", ["IsComplete"] = true },
        new() { ["TaskItemId"] = 3, ["TaskName"] = "Schedule RMAN backup", ["IsComplete"] = false },
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
        Console.WriteLine("  [Oracle] Reader closed.");
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
