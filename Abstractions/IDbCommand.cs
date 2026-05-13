namespace AbstractFactoryDemo.Abstractions;

/// Abstract Product B — represents an executable database command.
public interface IDbCommand
{
    string CommandText { get; }

    /// <summary>Adds a simple input parameter (shorthand for the most common case).</summary>
    void AddParameter(string name, object? value);

    /// <summary>Adds a fully-configured parameter (direction, type, size).</summary>
    void AddParameter(IDbParameter parameter);

    /// <summary>Returns a parameter by name, or null if not found.
    /// Use this after execution to read Output / ReturnValue results.</summary>
    IDbParameter? GetParameter(string name);

    /// <summary>Enlists the command in an active transaction.</summary>
    void SetTransaction(IDbTransaction transaction);

    IDbDataReader ExecuteReader();
    int           ExecuteNonQuery();
    object?       ExecuteScalar();
    Task<IDbDataReader> ExecuteReaderAsync(CancellationToken ct = default);
    Task<int>           ExecuteNonQueryAsync(CancellationToken ct = default);
    Task<object?>       ExecuteScalarAsync(CancellationToken ct = default);
}
