using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.SqlServer;

public class SqlServerParameter : IDbParameter
{
    public string               Name      { get; set; } = string.Empty;
    public object?              Value     { get; set; }
    public DbParameterDirection Direction { get; set; } = DbParameterDirection.Input;
    public DbType               DbType    { get; set; } = DbType.Object;
    public int                  Size      { get; set; } = 0;
    public object?              OutputValue { get; internal set; }
}
