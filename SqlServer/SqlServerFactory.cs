using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.SqlServer;

/// Concrete Factory A — creates the SQL Server product family.
public class SqlServerFactory : DatabaseFactoryBase
{
    public SqlServerFactory() : base("SQL Server", new SqlServerDialect()) { }

    public override IDbConnection CreateConnection(string connectionString)
        => new SqlServerConnection(connectionString);

    public override IDbCommand CreateCommand(string sql, IDbConnection connection)
        => new SqlServerCommand(sql, (SqlServerConnection)connection);

    public override IDbDataReader CreateDataReader(IDbCommand command)
        => ((SqlServerCommand)command).ExecuteReader();
}
