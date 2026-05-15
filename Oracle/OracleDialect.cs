using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Oracle;

public class OracleDialect : QueryDialectBase
{
    /// Oracle uses colon-prefixed bind variables instead of @.
    public override string ParameterPrefix => ":";

    public override string FormatParameter(string name) => $":{name}";

    /// Oracle 12c+ uses FETCH FIRST N ROWS ONLY appended to the query.
    public override string ApplyLimit(string sql, int count) => $"{sql} FETCH FIRST {count} ROWS ONLY";

    public override string BuildConnectionString(string host, string database, string? userId = null, string? password = null) =>
        $"Data Source={host}/{database};User Id={userId ?? "system"};Password={password ?? ""};";

    // Oracle uses "double-quote" identifiers for case-sensitive names
    protected override string Q(string name) => $"\"{name}\"";

    // Oracle uses MVCC by default — dirty reads are not possible, WITH (NOLOCK) doesn't exist
    protected override string NoLockHint => string.Empty;

    // Oracle 12c+ paging is identical to SQL Server ANSI syntax — no override needed
}
