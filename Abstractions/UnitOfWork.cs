namespace AbstractFactoryDemo.Abstractions;

/// Shared concrete Unit of Work — depends only on abstractions.
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDatabaseFactory _factory;
    private bool _disposed;

    public IDbConnection  Connection  { get; }
    public IDbTransaction Transaction { get; }

    internal UnitOfWork(IDatabaseFactory factory, IDbConnection connection, IDbTransaction transaction)
    {
        _factory    = factory;
        Connection  = connection;
        Transaction = transaction;
    }

    public IDbCommand CreateCommand(string sql)
    {
        var cmd = _factory.CreateCommand(sql, Connection);
        cmd.SetTransaction(Transaction);
        return cmd;
    }

    public Task CommitAsync(CancellationToken ct = default)   => Transaction.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => Transaction.RollbackAsync(ct);

    public void Dispose()
    {
        if (_disposed) return;
        Transaction.Dispose();
        Connection.Close();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        Transaction.Dispose();
        await Connection.CloseAsync();
        _disposed = true;
    }
}
