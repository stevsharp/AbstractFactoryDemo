namespace AbstractFactoryDemo.Abstractions;

/// Abstract base class for all database factories.
/// Owns ProviderName via constructor so concrete factories don't repeat it.
/// Subclasses implement the three product-creation methods.
public abstract class DatabaseFactoryBase(string providerName, IQueryDialect dialect) : IDatabaseFactory
{
    public string ProviderName { get; } = providerName;
    public IQueryDialect Dialect { get; } = dialect;
    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbCommand CreateCommand(string sql, IDbConnection connection);
    public abstract IDbDataReader CreateDataReader(IDbCommand command);
}
