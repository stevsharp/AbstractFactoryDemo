using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.SqlServer;

public class SqlServerDialect : IQueryDialect
{
    public string ParameterPrefix => "@";

    public string FormatParameter(string name) => $"@{name}";

    /// SQL Server uses SELECT TOP N syntax prepended after SELECT.
    public string ApplyLimit(string sql, int count) =>
        sql.Replace("SELECT ", $"SELECT TOP {count} ", StringComparison.OrdinalIgnoreCase);

    public string BuildConnectionString(string host, string database, string? userId = null, string? password = null) =>
        userId is null
            ? $"Server={host};Database={database};Trusted_Connection=True;"
            : $"Server={host};Database={database};User Id={userId};Password={password};";
}
