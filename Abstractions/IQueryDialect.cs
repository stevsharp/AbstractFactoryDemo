namespace AbstractFactoryDemo.Abstractions;

public interface IQueryDialect
{
    string ParameterPrefix { get; }

    string FormatParameter(string name);

    string ApplyLimit(string sql, int count);

    string BuildConnectionString(string host, string database, string? userId = null, string? password = null);

    // --- Query pattern builders ---

    string QuoteIdentifier(string name);

    string BuildSelectQuery(string table, IEnumerable<string> columns, string? whereClause = null, bool noLock = false);

    string BuildInsertQuery(string table, IEnumerable<string> columns);

    string BuildUpdateQuery(string table, IEnumerable<string> columns, string keyColumn);

    string BuildDeleteQuery(string table, string keyColumn);

    string ApplyPaging(string sql, int pageNumber, int pageSize, string orderBy = "1");
}
