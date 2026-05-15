using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.MySql;

public class MySqlDialect : QueryDialectBase
{
    public override string ParameterPrefix => "@";

    public override string FormatParameter(string name) => $"@{name}";

    /// MySQL appends LIMIT N at the end of the query.
    public override string ApplyLimit(string sql, int count) => $"{sql} LIMIT {count}";

    public override string BuildConnectionString(string host, string database, string? userId = null, string? password = null) =>
        $"Server={host};Database={database};Uid={userId ?? "root"};Pwd={password ?? ""};";

    // MySQL uses backtick quoting instead of [brackets]
    protected override string Q(string name) => $"`{name}`";

    // MySQL InnoDB uses MVCC — reads never block, WITH (NOLOCK) doesn't exist
    protected override string NoLockHint => string.Empty;

    // MySQL paging: LIMIT pageSize OFFSET offset
    public override string ApplyPaging(string sql, int pageNumber, int pageSize, string orderBy = "1")
    {
        int offset = (pageNumber - 1) * pageSize;
        return $"{sql} ORDER BY {orderBy} LIMIT {pageSize} OFFSET {offset}";
    }
}
