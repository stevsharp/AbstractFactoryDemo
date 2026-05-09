namespace AbstractFactoryDemo.Abstractions;

/// Abstract Product C — represents a forward-only result set reader.
public interface IDbDataReader
{
    bool Read();
    object GetValue(string columnName);
    void Close();
    Task<bool> ReadAsync(CancellationToken ct = default);
    Task CloseAsync(CancellationToken ct = default);
}
