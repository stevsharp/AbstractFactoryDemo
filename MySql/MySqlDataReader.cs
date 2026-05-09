using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.MySql;

/// Concrete Product C (MySQL) — simulates a MySQL result set.
public class MySqlDataReader : IDbDataReader
{
    private int _rowIndex = -1;

    private static readonly List<Dictionary<string, object>> Rows =
    [
        new() { ["TaskItemId"] = 1, ["TaskName"] = "Setup MySQL schema", ["IsComplete"] = false },
        new() { ["TaskItemId"] = 2, ["TaskName"] = "Add indexes", ["IsComplete"] = true },
        new() { ["TaskItemId"] = 3, ["TaskName"] = "Configure replication", ["IsComplete"] = false },
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
        Console.WriteLine("  [MySQL] Reader closed.");
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
