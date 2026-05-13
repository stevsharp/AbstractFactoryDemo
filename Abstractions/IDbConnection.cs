namespace AbstractFactoryDemo.Abstractions;

/// Abstract Product A — represents a database connection.
public interface IDbConnection
{
    string ConnectionString { get; }
    void Open();
    void Close();
    IDbTransaction BeginTransaction();
    Task OpenAsync(CancellationToken ct = default);
    Task CloseAsync(CancellationToken ct = default);
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
