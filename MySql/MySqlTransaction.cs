using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.MySql;

public class MySqlTransaction : IDbTransaction
{
    private bool _isActive = true;

    public bool IsActive => _isActive;

    public void Commit()
    {
        EnsureActive();
        Console.WriteLine("  [MySQL] Transaction committed.");
        _isActive = false;
    }

    public void Rollback()
    {
        EnsureActive();
        Console.WriteLine("  [MySQL] Transaction rolled back.");
        _isActive = false;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Commit();
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Rollback();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_isActive)
        {
            Console.WriteLine("  [MySQL] Transaction disposed without commit — auto-rollback.");
            _isActive = false;
        }
    }

    private void EnsureActive()
    {
        if (!_isActive)
            throw new InvalidOperationException("Transaction is no longer active.");
    }
}
