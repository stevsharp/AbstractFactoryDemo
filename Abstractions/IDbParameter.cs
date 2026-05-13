namespace AbstractFactoryDemo.Abstractions;

/// Abstract Product — a typed, directional command parameter.
public interface IDbParameter
{
    string               Name      { get; set; }
    object?              Value     { get; set; }
    DbParameterDirection Direction { get; set; }
    DbType               DbType    { get; set; }
    /// <summary>0 = use provider default (e.g. for strings: max length).</summary>
    int                  Size      { get; set; }
    /// <summary>Populated after command execution for Output / InputOutput / ReturnValue params.</summary>
    object?              OutputValue { get; }
}
