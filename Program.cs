using Microsoft.Extensions.Configuration;
using AbstractFactoryDemo.Abstractions;
using AbstractFactoryDemo.Client;
using AbstractFactoryDemo.SqlServer;
using AbstractFactoryDemo.MySql;
using AbstractFactoryDemo.Oracle;

// Load configuration
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// Honour Ctrl+C for the entire run
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\n[!] Cancellation requested...");
    cts.Cancel();
};

Console.WriteLine("==============================================");
Console.WriteLine("  Abstract Factory Pattern — Database Demo");
Console.WriteLine("  Table: TaskItem");
Console.WriteLine("==============================================");
Console.WriteLine("(Press Ctrl+C at any time to cancel)");
Console.WriteLine();

Console.WriteLine("Choose a database provider:");
Console.WriteLine("1. SQL Server");
Console.WriteLine("2. MySQL");
Console.WriteLine("3. Oracle");
Console.Write("Enter your choice (1-3): ");

string? choice = Console.ReadLine()?.Trim();

IDatabaseFactory factory;
string connectionString;
string procSql;  // stored-proc syntax varies per provider

switch (choice)
{
    case "1":
        Console.WriteLine("\nUsing SQL Server...");
        factory = new SqlServerFactory();
        connectionString = config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SqlServer connection string not found.");
        procSql = "EXEC sp_GetTaskCount @Filter, @Count OUTPUT, @ReturnCode";
        break;

    case "2":
        Console.WriteLine("\nUsing MySQL...");
        factory = new MySqlFactory();
        connectionString = config.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found.");
        procSql = "CALL sp_GetTaskCount(@Filter, @Count, @ReturnCode)";
        break;

    case "3":
        Console.WriteLine("\nUsing Oracle...");
        factory = new OracleFactory();
        connectionString = config.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("Oracle connection string not found.");
        procSql = "BEGIN sp_GetTaskCount(:Filter, :Count, :ReturnCode); END;";
        break;

    default:
        Console.WriteLine("Invalid choice. Exiting...");
        return;
}

// ── Build all SQL via the dialect (provider-agnostic) ─────────────────────────
var d     = factory.Dialect;
var table = "TaskItem";
string[] readCols = ["TaskItemId", "TaskName", "IsComplete"];

string selectSql     = d.BuildSelectQuery(table, readCols);
string selectNoLock  = d.BuildSelectQuery(table, readCols, noLock: true);
string selectPaged   = d.ApplyPaging(d.BuildSelectQuery(table, readCols), pageNumber: 1, pageSize: 5, orderBy: "TaskItemId");
string selectWhere   = d.BuildSelectQuery(table, readCols, whereClause: $"{d.QuoteIdentifier("IsComplete")} = 0");
string insertSql     = d.BuildInsertQuery(table, ["TaskName", "IsComplete"]);
string updateSql     = d.BuildUpdateQuery(table, ["IsComplete"], keyColumn: "TaskItemId");
string deleteSql     = d.BuildDeleteQuery(table, keyColumn: "TaskItemId");
string countSql      = $"SELECT COUNT(*) FROM {d.QuoteIdentifier(table)}";

Console.WriteLine("\n--- Generated SQL for this provider ---");
Console.WriteLine($"  SELECT       : {selectSql}");
Console.WriteLine($"  SELECT NOLOCK: {selectNoLock}");
Console.WriteLine($"  SELECT PAGED : {selectPaged}");
Console.WriteLine($"  SELECT WHERE : {selectWhere}");
Console.WriteLine($"  INSERT       : {insertSql}");
Console.WriteLine($"  UPDATE       : {updateSql}");
Console.WriteLine($"  DELETE       : {deleteSql}");
Console.WriteLine($"  COUNT        : {countSql}");

// ─────────────────────────────────────────────────────────────────────────────

var client = new DatabaseClient(factory);

Console.WriteLine("\nRunning operations...");

try
{
    // ── Basic CRUD ───────────────────────────────────────────────────────────

    // SELECT (normal — shared lock)
    await client.RunQueryAsync(connectionString, selectSql, ct: cts.Token);

    // SELECT WITH NOLOCK — SQL Server emits WITH (NOLOCK), MySQL/Oracle emit nothing
    Console.WriteLine($"\n--- NOLOCK SELECT via {factory.ProviderName} ---");
    Console.WriteLine($"  SQL: {selectNoLock}");
    await client.RunQueryAsync(connectionString, selectNoLock, ct: cts.Token);

    // SELECT with WHERE clause
    Console.WriteLine($"\n--- WHERE clause SELECT via {factory.ProviderName} ---");
    Console.WriteLine($"  SQL: {selectWhere}");
    await client.RunQueryAsync(connectionString, selectWhere, ct: cts.Token);

    // SELECT paged
    Console.WriteLine($"\n--- PAGED SELECT via {factory.ProviderName} ---");
    Console.WriteLine($"  SQL: {selectPaged}");
    await client.RunQueryAsync(connectionString, selectPaged, ct: cts.Token);

    // INSERT
    await client.RunInsertAsync(connectionString, insertSql,
        taskName:   "New task from Abstract Factory demo",
        isComplete: false,
        ct: cts.Token);

    // UPDATE — mark TaskItemId = 1 as complete
    await client.RunUpdateAsync(connectionString, updateSql,
        taskItemId: 1,
        isComplete: true,
        ct: cts.Token);

    // COUNT
    await client.RunScalarAsync(connectionString, countSql, ct: cts.Token);

    // ── Unit of Work (atomic multi-statement transaction) ────────────────────

    await client.RunUnitOfWorkAsync(
        connectionString,
        insertSql:         insertSql,
        taskName:          "UoW Task — part of atomic batch",
        isComplete:        false,
        updateSql:         updateSql,
        taskItemId:        2,
        updatedIsComplete: true,
        ct: cts.Token);

    // ── Stored procedure with Input / Output / ReturnValue params ────────────

    await client.RunStoredProcAsync(
        connectionString,
        procSql:     procSql,
        filterValue: "active",
        ct: cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("[!] Operation was cancelled.");
    return;
}

Console.WriteLine("\n==============================================");
Console.WriteLine("Pattern Used: Abstract Factory + Unit of Work");
Console.WriteLine("==============================================");
Console.WriteLine($"Selected Factory : {factory.GetType().Name}");
Console.WriteLine($"Dialect          : {factory.Dialect.GetType().Name}");
Console.WriteLine($"Parameter prefix : {factory.Dialect.FormatParameter("x")}");
Console.WriteLine($"NOLOCK hint      : '{factory.Dialect.BuildSelectQuery("T", ["c"], noLock: true)}'");
Console.WriteLine("DatabaseClient works only with abstractions.");
Console.WriteLine("Concrete implementations are chosen at runtime.");
Console.ReadLine();
