# Abstract Factory Pattern in C#

Read the full article at : 
(https://coderlegion.com/17410/abstract-factory-pattern-tutorial)

> A real-world implementation of the **Abstract Factory Design Pattern** using a multi-database access layer that supports:

* Microsoft SQL Server
* MySQL MySQL
* Oracle Corporation Oracle

This project demonstrates how to build a **database-agnostic system** where the client works only with abstractions and can switch providers at runtime.

---

## Why This Project?

Many applications start with code like this:

```csharp id="oq7gdw"
if(provider == "sqlserver")
{
    connection = new SqlConnection(...);
}
else if(provider == "mysql")
{
    connection = new MySqlConnection(...);
}
else if(provider == "oracle")
{
    connection = new OracleConnection(...);
}
```

This causes:

* Tight coupling
* Repeated conditional logic
* Difficult testing
* Poor scalability
* Open/Closed Principle violations

This project solves that using the **Abstract Factory Pattern**.

---

# Pattern Definition

According to the Gang of Four:

> "Provide an interface for creating families of related or dependent objects without specifying their concrete classes."

— Design Patterns: Elements of Reusable Object-Oriented Software

---

# Architecture

```text id="b2fqz7"
                    IDatabaseFactory
                           |
    ------------------------------------------------
    |                     |                       |
SqlServerFactory     MySqlFactory         OracleFactory
    |                     |                       |
Connection          Connection            Connection
Command             Command               Command
Reader              Reader                Reader
Dialect             Dialect               Dialect
```

---

# Project Structure

```bash id="79khh7"
AbstractFactoryDemo/
│
├── Abstractions/
│   ├── IDatabaseFactory.cs
│   ├── DatabaseFactoryBase.cs      ← base factory (handles UoW creation)
│   ├── IUnitOfWork.cs              ← Unit of Work interface
│   ├── UnitOfWork.cs               ← shared Unit of Work implementation
│   ├── IDbConnection.cs
│   ├── IDbCommand.cs
│   ├── IDbDataReader.cs
│   ├── IDbParameter.cs
│   ├── IDbTransaction.cs
│   ├── IQueryDialect.cs
│   ├── DbParameterDirection.cs
│   └── DbType.cs
│
├── SqlServer/
│   ├── SqlServerFactory.cs
│   ├── SqlServerConnection.cs
│   ├── SqlServerCommand.cs
│   ├── SqlServerDataReader.cs
│   ├── SqlServerParameter.cs
│   ├── SqlServerTransaction.cs
│   └── SqlServerDialect.cs
│
├── MySql/
│   ├── MySqlFactory.cs
│   ├── MySqlConnection.cs
│   ├── MySqlCommand.cs
│   ├── MySqlDataReader.cs
│   ├── MySqlParameter.cs
│   ├── MySqlTransaction.cs
│   └── MySqlDialect.cs
│
├── Oracle/
│   ├── OracleFactory.cs
│   ├── OracleConnection.cs
│   ├── OracleCommand.cs
│   ├── OracleDataReader.cs
│   ├── OracleParameter.cs
│   ├── OracleTransaction.cs
│   └── OracleDialect.cs
│
├── Client/
│   └── DatabaseClient.cs
│
├── appsettings.json
└── Program.cs
```

---

# How It Works

Each factory creates a family of compatible objects:

| Factory          | Creates            |
| ---------------- | ------------------ |
| SqlServerFactory | SQL Server objects |
| MySqlFactory     | MySQL objects      |
| OracleFactory    | Oracle objects     |

The client never directly creates concrete implementations.

---

# Database Factory Interface

```csharp id="h6gvsp"
public interface IDatabaseFactory
{
    string ProviderName { get; }

    IQueryDialect Dialect { get; }

    IDbConnection CreateConnection(string connectionString);

    IDbCommand CreateCommand(
        string sql,
        IDbConnection connection);

    IDbDataReader CreateDataReader(
        IDbCommand command);
}
```

---

# SQL Dialect Interface

Different databases use different SQL syntax.

```csharp id="uv7z5s"
public interface IQueryDialect
{
    string ApplyLimit(string sql, int limit);

    string FormatParameter(string name);

    string BuildConnectionString(
        string host,
        string database,
        string? user = null,
        string? password = null);
}
```

---

# Database Client

The client works only with abstractions.

```csharp id="vl3lyu"
public class DatabaseClient
{
    private readonly IDatabaseFactory _factory;

    public DatabaseClient(IDatabaseFactory factory)
    {
        _factory = factory;
    }
}
```

---

# Unit of Work

The **Unit of Work** pattern groups multiple database operations into a single transaction, committing or rolling back atomically.

## Interface

```csharp
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IDbConnection  Connection  { get; }
    IDbTransaction Transaction { get; }

    IDbCommand CreateCommand(string sql);

    Task CommitAsync  (CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
```

## Creating a Unit of Work

Call `CreateUnitOfWorkAsync` on any factory — it opens the connection and begins the transaction for you:

```csharp
// Async (preferred)
await using var uow = await _factory.CreateUnitOfWorkAsync(connectionString);

// Sync
using var uow = _factory.CreateUnitOfWork(connectionString);
```

## Using a Unit of Work

All commands created from the same `IUnitOfWork` share the same transaction:

```csharp
await using var uow = await _factory.CreateUnitOfWorkAsync(connectionString);
try
{
    // First operation — INSERT
    var insert = uow.CreateCommand(
        "INSERT INTO [TaskItem] ([TaskName], [IsComplete]) VALUES (@TaskName, @IsComplete)");
    insert.AddParameter("@TaskName",   "Buy groceries");
    insert.AddParameter("@IsComplete", false);
    await insert.ExecuteNonQueryAsync();

    // Second operation — UPDATE (same transaction)
    var update = uow.CreateCommand(
        "UPDATE [TaskItem] SET [IsComplete] = @IsComplete WHERE [TaskItemId] = @TaskItemId");
    update.AddParameter("@IsComplete",  true);
    update.AddParameter("@TaskItemId",  42);
    await update.ExecuteNonQueryAsync();

    // Both succeed — commit
    await uow.CommitAsync();
}
catch
{
    // Either failed — rollback both
    await uow.RollbackAsync();
    throw;
}
```

`await using` (or `using`) ensures the connection is always closed, even if an exception is thrown before `CommitAsync`.

---

# SQL Server Query Patterns

This section shows how to write queries against SQL Server using the factory abstractions.

## Parameter Format

SQL Server parameters use the `@` prefix:

```csharp
command.AddParameter("@TaskName",   "Buy groceries");
command.AddParameter("@IsComplete", false);
command.AddParameter("@TaskItemId", 1);
```

## SELECT — Read Rows

```csharp
var conn = _factory.CreateConnection(connectionString);
conn.Open();

// Limit is applied as SELECT TOP N by the SQL Server dialect
var sql    = _factory.Dialect.ApplyLimit(
                 "SELECT [TaskItemId], [TaskName], [IsComplete] FROM [TaskItem]", 10);
var cmd    = _factory.CreateCommand(sql, conn);
var reader = _factory.CreateDataReader(cmd);

while (await reader.ReadAsync())
{
    int    id         = reader.GetInt32 (0);
    string taskName   = reader.GetString(1);
    bool   isComplete = reader.GetBoolean(2);
    Console.WriteLine($"Id={id}  Name={taskName}  Done={isComplete}");
}
```

`ApplyLimit` for SQL Server rewrites the query to `SELECT TOP 10 [TaskItemId] ...`.

## INSERT — Write a Row

```csharp
var cmd = _factory.CreateCommand(
    "INSERT INTO [TaskItem] ([TaskName], [IsComplete]) VALUES (@TaskName, @IsComplete)",
    conn);

cmd.AddParameter("@TaskName",   "Buy groceries");
cmd.AddParameter("@IsComplete", false);

int rowsAffected = await cmd.ExecuteNonQueryAsync();
```

## UPDATE — Modify a Row

```csharp
var cmd = _factory.CreateCommand(
    "UPDATE [TaskItem] SET [IsComplete] = @IsComplete WHERE [TaskItemId] = @TaskItemId",
    conn);

cmd.AddParameter("@IsComplete",  true);
cmd.AddParameter("@TaskItemId",  1);

int rowsAffected = await cmd.ExecuteNonQueryAsync();
```

## Scalar — Aggregate Value

```csharp
var cmd    = _factory.CreateCommand("SELECT COUNT(*) FROM [TaskItem]", conn);
object? result = await cmd.ExecuteScalarAsync();
int count  = Convert.ToInt32(result);
```

## Stored Procedure — Input / Output / Return Parameters

Use `IDbParameter` directly when you need direction, type, or size control:

```csharp
var cmd = _factory.CreateCommand("EXEC sp_GetTaskCount @Filter, @Count OUTPUT, @ReturnCode", conn);

// Input parameter
var filterParam = _factory.CreateParameter(
    "@Filter", "pending",
    DbParameterDirection.Input, DbType.String, size: 50);

// Output parameter — value populated after execution
var countParam = _factory.CreateParameter(
    "@Count", null,
    DbParameterDirection.Output, DbType.Int32);

// Return value from the stored procedure
var returnParam = _factory.CreateParameter(
    "@ReturnCode", null,
    DbParameterDirection.ReturnValue, DbType.Int32);

cmd.AddParameter(filterParam);
cmd.AddParameter(countParam);
cmd.AddParameter(returnParam);

await cmd.ExecuteNonQueryAsync();

int count      = Convert.ToInt32(countParam.Value);
int returnCode = Convert.ToInt32(returnParam.Value);
```

## Connection String Builder

Use the SQL Server dialect to construct a connection string at runtime:

```csharp
// Windows integrated authentication
string cs = _factory.Dialect.BuildConnectionString(
    host:     "localhost",
    database: "MyDb");
// → "Server=localhost;Database=MyDb;Trusted_Connection=True;"

// SQL login
string cs = _factory.Dialect.BuildConnectionString(
    host:     "prod-server",
    database: "MyDb",
    user:     "sa",
    password: "secret");
// → "Server=prod-server;Database=MyDb;User Id=sa;Password=secret;"
```

---

# Running the Project

Clone the repository:

```bash id="kllgwj"
git clone https://github.com/yourusername/AbstractFactoryDemo.git
cd AbstractFactoryDemo
```

Run the application:

```bash id="w0j4b2"
dotnet run
```

---

# Runtime Provider Selection

The application asks the user which database to use:

```text id="31e8p6"
Choose a database provider:
1. SQL Server
2. MySQL
3. Oracle
```

Example:

```csharp id="r4b6ix"
switch(choice)
{
    case "1":
        factory = new SqlServerFactory();
        break;

    case "2":
        factory = new MySqlFactory();
        break;

    case "3":
        factory = new OracleFactory();
        break;
}
```

This is acceptable because it happens at the **composition root**.

---

# Example Operations

```csharp id="3y5v3w"
await client.RunQueryAsync(...);
await client.RunInsertAsync(...);
await client.RunUpdateAsync(...);
await client.RunScalarAsync(...);
await client.RunUnitOfWorkAsync(...);   // atomic multi-statement transaction
await client.RunStoredProcAsync(...);   // stored proc with output/return params
```

---

# Sample Output

```text id="faj4qe"
==============================================
Abstract Factory Pattern — Database Demo
==============================================

Choose a database provider:
1. SQL Server
2. MySQL
3. Oracle

Enter your choice: 2

Using MySQL...

Running operations...

--- RunQuery via MySQL ---
Row: Id=1 Name=Alice

--- RunNonQuery via MySQL ---
Rows affected: 1

--- RunScalar via MySQL ---
Scalar result: 15
```

---

# Adding a New Provider

Want to support PostgreSQL Global Development Group?

Add:

* PostgreSqlFactory
* PostgreSqlConnection
* PostgreSqlCommand
* PostgreSqlReader
* PostgreSqlDialect

No changes needed in:

* `DatabaseClient`
* Business logic
* Existing factories

---

# Benefits

* ✅ Open/Closed Principle
* ✅ Easy testing
* ✅ Scalable architecture
* ✅ Clean separation of concerns
* ✅ Runtime flexibility

---

# When NOT to Use Abstract Factory

Avoid this pattern when:

* You support only one database
* You don’t have multiple object families
* A simple factory method is enough

---

# References

### Books

* Design Patterns: Elements of Reusable Object-Oriented Software
* Clean Architecture by Robert C. Martin

---

### Official Documentation

* [Microsoft ADO.NET Docs](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/?utm_source=chatgpt.com)
* [Microsoft Async Programming Docs](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/?utm_source=chatgpt.com)
* [MySQL Docs](https://dev.mysql.com/doc/?utm_source=chatgpt.com)
* [Oracle Docs](https://www.oracle.com/database/technologies/?utm_source=chatgpt.com)

---

# Future Improvements

Potential enhancements:

* Dependency Injection version
* Unit tests with fake factories
* Docker database containers
* PostgreSQL implementation
* Entity Framework version
* Repository pattern layered on top of Unit of Work

---

# Contributing

Pull requests are welcome.

For major changes:

1. Fork the repo
2. Create a feature branch
3. Submit a PR

---

# License

MIT License
