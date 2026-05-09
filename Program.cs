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
string selectSql, insertSql, updateSql, countSql;

switch (choice)
{
    case "1":
        Console.WriteLine("\nUsing SQL Server...");
        factory = new SqlServerFactory();
        connectionString = config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SqlServer connection string not found in appsettings.json.");

        selectSql = "SELECT [TaskItemId], [TaskName], [IsComplete] FROM [TaskItem]";
        insertSql = "INSERT INTO [TaskItem] ([TaskName], [IsComplete]) VALUES (@TaskName, @IsComplete)";
        updateSql = "UPDATE [TaskItem] SET [IsComplete] = @IsComplete WHERE [TaskItemId] = @TaskItemId";
        countSql  = "SELECT COUNT(*) FROM [TaskItem]";
        break;

    case "2":
        Console.WriteLine("\nUsing MySQL...");
        factory = new MySqlFactory();
        connectionString = config.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found in appsettings.json.");

        selectSql = "SELECT TaskItemId, TaskName, IsComplete FROM TaskItem";
        insertSql = "INSERT INTO TaskItem (TaskName, IsComplete) VALUES (@TaskName, @IsComplete)";
        updateSql = "UPDATE TaskItem SET IsComplete = @IsComplete WHERE TaskItemId = @TaskItemId";
        countSql  = "SELECT COUNT(*) FROM TaskItem";
        break;

    case "3":
        Console.WriteLine("\nUsing Oracle...");
        factory = new OracleFactory();
        connectionString = config.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("Oracle connection string not found in appsettings.json.");

        selectSql = "SELECT TaskItemId, TaskName, IsComplete FROM TaskItem";
        insertSql = "INSERT INTO TaskItem (TaskName, IsComplete) VALUES (:TaskName, :IsComplete)";
        updateSql = "UPDATE TaskItem SET IsComplete = :IsComplete WHERE TaskItemId = :TaskItemId";
        countSql  = "SELECT COUNT(*) FROM TaskItem";
        break;

    default:
        Console.WriteLine("Invalid choice. Exiting...");
        return;
}

var client = new DatabaseClient(factory);

Console.WriteLine("\nRunning operations...");


try
{
    // SELECT
    await client.RunQueryAsync(connectionString, selectSql, ct: cts.Token);

    // INSERT
    await client.RunInsertAsync(connectionString, insertSql,
        taskName: "New task from Abstract Factory demo",
        isComplete: false,
        ct: cts.Token);

    // UPDATE — mark TaskItemId = 1 as complete
    await client.RunUpdateAsync(connectionString, updateSql,
        taskItemId: 1,
        isComplete: true,
        ct: cts.Token);

    // COUNT
    await client.RunScalarAsync(connectionString, countSql, ct: cts.Token);

    // SELECT again to show state after insert + update
    await client.RunQueryAsync(connectionString, selectSql, ct: cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("[!] Operation was cancelled.");
    return;
}

Console.WriteLine("\n==============================================");
Console.WriteLine("Pattern Used: Abstract Factory");
Console.WriteLine("==============================================");
Console.WriteLine($"Selected Factory : {factory.GetType().Name}");
Console.WriteLine($"Dialect          : {factory.Dialect.GetType().Name}");
Console.WriteLine($"Parameter prefix : {factory.Dialect.FormatParameter("x")}");
Console.WriteLine("DatabaseClient works only with abstractions.");
Console.WriteLine("Concrete implementations are chosen at runtime.");
Console.ReadLine();