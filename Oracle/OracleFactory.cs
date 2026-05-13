using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Oracle;

/// Concrete Factory C — creates the Oracle product family.
public class OracleFactory : DatabaseFactoryBase
{
    public OracleFactory() : base("Oracle", new OracleDialect()) { }

    public override IDbConnection CreateConnection(string connectionString)
        => new OracleConnection(connectionString);

    public override IDbCommand CreateCommand(string sql, IDbConnection connection)
        => new OracleCommand(sql, (OracleConnection)connection);

    public override IDbDataReader CreateDataReader(IDbCommand command)
        => ((OracleCommand)command).ExecuteReader();

    public override IDbParameter CreateParameter(
        string name, object? value,
        DbParameterDirection direction = DbParameterDirection.Input,
        DbType dbType = DbType.Object,
        int size = 0)
        => new OracleParameter { Name = name, Value = value, Direction = direction, DbType = dbType, Size = size };
}
