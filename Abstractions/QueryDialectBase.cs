namespace AbstractFactoryDemo.Abstractions;

/// Abstract base for all query dialects.
/// SQL Server syntax is the default — providers override only what differs.
public abstract class QueryDialectBase : IQueryDialect
{
    // --- Must be implemented by every provider ---

    public abstract string ParameterPrefix { get; }
    public abstract string FormatParameter(string name);
    public abstract string ApplyLimit(string sql, int count);
    public abstract string BuildConnectionString(string host, string database, string? userId = null, string? password = null);

    // --- Identifier quoting: SQL Server [name], overridden per provider ---

    protected virtual string Q(string name) => $"[{name}]";

    /// Public surface so callers can quote identifiers without knowing the provider.
    public string QuoteIdentifier(string name) => Q(name);

    // --- NOLOCK hint: SQL Server WITH (NOLOCK), empty string for providers that don't need it ---

    /// MySQL and Oracle use MVCC — their reads never block, so they override this to empty.
    protected virtual string NoLockHint => " WITH (NOLOCK)";

    // --- Query pattern builders: SQL Server defaults, override per provider ---

    public virtual string BuildSelectQuery(
        string table,
        IEnumerable<string> columns,
        string? whereClause = null,
        bool noLock = false)
    {
        var cols     = string.Join(", ", columns.Select(Q));
        var tableRef = noLock ? $"{Q(table)}{NoLockHint}" : Q(table);
        var sql      = $"SELECT {cols} FROM {tableRef}";
        return whereClause is null ? sql : $"{sql} WHERE {whereClause}";
    }

    public virtual string BuildInsertQuery(string table, IEnumerable<string> columns)
    {
        var cols      = columns.ToList();
        var colList   = string.Join(", ", cols.Select(Q));
        var paramList = string.Join(", ", cols.Select(FormatParameter));
        return $"INSERT INTO {Q(table)} ({colList}) VALUES ({paramList})";
    }

    public virtual string BuildUpdateQuery(string table, IEnumerable<string> columns, string keyColumn)
    {
        var setClauses = string.Join(", ", columns.Select(c => $"{Q(c)} = {FormatParameter(c)}"));
        return $"UPDATE {Q(table)} SET {setClauses} WHERE {Q(keyColumn)} = {FormatParameter(keyColumn)}";
    }

    public virtual string BuildDeleteQuery(string table, string keyColumn)
        => $"DELETE FROM {Q(table)} WHERE {Q(keyColumn)} = {FormatParameter(keyColumn)}";

    /// SQL Server 2012+ / ANSI — OFFSET … FETCH NEXT … ROWS ONLY
    public virtual string ApplyPaging(string sql, int pageNumber, int pageSize, string orderBy = "1")
    {
        int offset = (pageNumber - 1) * pageSize;
        return $"{sql} ORDER BY {orderBy} OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
    }
}
