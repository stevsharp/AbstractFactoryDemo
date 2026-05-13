namespace AbstractFactoryDemo.Abstractions;

/// Abstract base class for all database factories.
public abstract class DatabaseFactoryBase(string providerName, IQueryDialect dialect) : IDatabaseFactory
{
    public string        ProviderName { get; } = providerName;
    public IQueryDialect Dialect      { get; } = dialect;

    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbCommand    CreateCommand(string sql, IDbConnection connection);
    public abstract IDbDataReader CreateDataReader(IDbCommand command);
    public abstract IDbParameter  CreateParameter(
        string               name,
        object?              value,
        DbParameterDirection direction = DbParameterDirection.Input,
        DbType               dbType    = DbType.Object,
        int                  size      = 0);

    public IUnitOfWork CreateUnitOfWork(string connectionString)
    {
        var conn = CreateConnection(connectionString);
        conn.Open();
        var tx = conn.BeginTransaction();
        return new UnitOfWork(this, conn, tx);
    }

    public async Task<IUnitOfWork> CreateUnitOfWorkAsync(string connectionString, CancellationToken ct = default)
    {
        var conn = CreateConnection(connectionString);
        await conn.OpenAsync(ct);
        var tx = await conn.BeginTransactionAsync(ct);
        return new UnitOfWork(this, conn, tx);
    }
}
