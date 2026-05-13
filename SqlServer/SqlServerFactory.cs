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

    public override IDbParameter CreateParameter(
        string name, object? value,
        DbParameterDirection direction = DbParameterDirection.Input,
        DbType dbType = DbType.Object,
        int size = 0)
        => new SqlServerParameter { Name = name, Value = value, Direction = direction, DbType = dbType, Size = size };
}
