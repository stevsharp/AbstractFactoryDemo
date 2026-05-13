namespace AbstractFactoryDemo.Abstractions;

/// Abstract Product — a database transaction that can be committed or rolled back.
public interface IDbTransaction : IDisposable
{
    bool IsActive { get; }
    void Commit();
    void Rollback();
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
