namespace AbstractFactoryDemo.Abstractions;

/// Unit of Work — groups multiple commands under one connection + transaction.
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IDbConnection  Connection  { get; }
    IDbTransaction Transaction { get; }

    /// <summary>Creates a command already enlisted in the active transaction.</summary>
    IDbCommand CreateCommand(string sql);

    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
