using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Oracle;

public class OracleDialect : IQueryDialect
{
    /// Oracle uses colon-prefixed bind variables instead of @.
    public string ParameterPrefix => ":";

    public string FormatParameter(string name) => $":{name}";

    /// Oracle 12c+ uses FETCH FIRST N ROWS ONLY appended to the query.
    public string ApplyLimit(string sql, int count) => $"{sql} FETCH FIRST {count} ROWS ONLY";

    public string BuildConnectionString(string host, string database, string? userId = null, string? password = null) =>
        $"Data Source={host}/{database};User Id={userId ?? "system"};Password={password ?? ""};";
}
