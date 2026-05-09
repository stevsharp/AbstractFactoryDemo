namespace AbstractFactoryDemo.Abstractions;

/// Abstract Product B — represents an executable database command.
public interface IDbCommand
{
    string CommandText { get; }
    void AddParameter(string name, object value);
    IDbDataReader ExecuteReader();
    int ExecuteNonQuery();
    object? ExecuteScalar();
    Task<IDbDataReader> ExecuteReaderAsync(CancellationToken ct = default);
    Task<int> ExecuteNonQueryAsync(CancellationToken ct = default);
    Task<object?> ExecuteScalarAsync(CancellationToken ct = default);
}
