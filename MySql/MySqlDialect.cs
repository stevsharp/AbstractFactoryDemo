using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.MySql;

public class MySqlDialect : IQueryDialect
{
    public string ParameterPrefix => "@";

    public string FormatParameter(string name) => $"@{name}";

    /// MySQL appends LIMIT N at the end of the query.
    public string ApplyLimit(string sql, int count) => $"{sql} LIMIT {count}";

    public string BuildConnectionString(string host, string database, string? userId = null, string? password = null) =>
        $"Server={host};Database={database};Uid={userId ?? "root"};Pwd={password ?? ""};";
}
