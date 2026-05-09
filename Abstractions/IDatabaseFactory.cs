namespace AbstractFactoryDemo.Abstractions;

public interface IDatabaseFactory
{
    string ProviderName { get; }
    IQueryDialect Dialect { get; }
    IDbConnection CreateConnection(string connectionString);
    IDbCommand CreateCommand(string sql, IDbConnection connection);
    IDbDataReader CreateDataReader(IDbCommand command);
}
