using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.MySql;

/// Concrete Factory B — creates the MySQL product family.
public class MySqlFactory : DatabaseFactoryBase
{
    public MySqlFactory() : base("MySQL", new MySqlDialect()) { }

    public override IDbConnection CreateConnection(string connectionString)
        => new MySqlConnection(connectionString);

    public override IDbCommand CreateCommand(string sql, IDbConnection connection)
        => new MySqlCommand(sql, (MySqlConnection)connection);

    public override IDbDataReader CreateDataReader(IDbCommand command)
        => ((MySqlCommand)command).ExecuteReader();
}
