namespace AbstractFactoryDemo.Abstractions;

public interface IQueryDialect
{
    string ParameterPrefix { get; }

    string FormatParameter(string name);

    string ApplyLimit(string sql, int count);

    string BuildConnectionString(string host, string database, string? userId = null, string? password = null);
}
