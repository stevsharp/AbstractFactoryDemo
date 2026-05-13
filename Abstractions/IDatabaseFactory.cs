namespace AbstractFactoryDemo.Abstractions;

public interface IDatabaseFactory
{
    string        ProviderName { get; }
    IQueryDialect Dialect      { get; }

    IDbConnection CreateConnection(string connectionString);
    IDbCommand    CreateCommand(string sql, IDbConnection connection);
    IDbDataReader CreateDataReader(IDbCommand command);

    IDbParameter CreateParameter(
        string               name,
        object?              value,
        DbParameterDirection direction = DbParameterDirection.Input,
        DbType               dbType    = DbType.Object,
        int                  size      = 0);

    IUnitOfWork       CreateUnitOfWork(string connectionString);
    Task<IUnitOfWork> CreateUnitOfWorkAsync(string connectionString, CancellationToken ct = default);
}
