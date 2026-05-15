using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.SqlServer;

/// SQL Server dialect.
/// Query-builder defaults in QueryDialectBase already use SQL Server syntax,
/// so only the four provider-specific members need implementing here.
public class SqlServerDialect : QueryDialectBase
{
    public override string ParameterPrefix => "@";

    public override string FormatParameter(string name) => $"@{name}";

    /// SQL Server uses SELECT TOP N prepended after SELECT.
    public override string ApplyLimit(string sql, int count) =>
        sql.Replace("SELECT ", $"SELECT TOP {count} ", StringComparison.OrdinalIgnoreCase);

    public override string BuildConnectionString(string host, string database, string? userId = null, string? password = null) =>
        userId is null
            ? $"Server={host};Database={database};Trusted_Connection=True;"
            : $"Server={host};Database={database};User Id={userId};Password={password};";
}
