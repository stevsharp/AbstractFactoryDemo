---
# Abstract Factory Pattern in C#

## A Step-by-Step Multi-Database Tutorial (SQL Server, MySQL, Oracle)

> Build a database-agnostic data access layer using the **Abstract Factory Pattern** in C#.

By the end of this tutorial, you’ll understand:

* What problem Abstract Factory solves
* Why naive implementations fail
* How to design families of related database objects
* How to support multiple database providers cleanly
* How to scale the design for future providers

---

# Table of Contents

1. Introduction
2. The Problem
3. Why Traditional Approaches Fail
4. What Abstract Factory Is
5. System Design
6. Creating the Factory Contract
7. Creating the Dialect Contract
8. Building Concrete Factories
9. Implementing the Client
10. Application Entry Point
11. Running the Application
12. Adding a New Database Provider
13. Benefits
14. Drawbacks
15. When NOT to Use Abstract Factory
16. References

---

# 1. Introduction

Modern enterprise systems often support multiple databases:

* Microsoft SQL Server
* MySQL MySQL
* Oracle Corporation Oracle

This happens when:

* Different clients use different providers
* Companies migrate databases
* SaaS applications support customer preferences
* Legacy systems must remain compatible

Without proper design, database logic becomes tightly coupled to application code.

---

# 2. The Problem

Many developers start like this:

```csharp id="i6c1v8"
if(provider == "sqlserver")
{
    connection = new SqlConnection(connectionString);
}
else if(provider == "mysql")
{
    connection = new MySqlConnection(connectionString);
}
else if(provider == "oracle")
{
    connection = new OracleConnection(connectionString);
}
```

Then SQL differences appear:

```csharp id="6vce3e"
if(provider == "mysql")
{
    sql = "SELECT * FROM Users LIMIT 10";
}
else if(provider == "oracle")
{
    sql = "SELECT * FROM Users FETCH FIRST 10 ROWS ONLY";
}
```

And now your business logic knows too much about infrastructure.

---

# 3. Why This Approach Fails

### Tight Coupling

The client depends on all providers.

### Poor Scalability

Adding PostgreSQL means changing existing code.

### Hard Testing

Mocking becomes difficult.

### Open/Closed Principle Violation

Your code must be modified every time a provider changes.

Reference: Robert C. Martin introduced the Open/Closed Principle in *Clean Architecture*.

---

# 4. What Is Abstract Factory?

According to the **Gang of Four**:

> “Provide an interface for creating families of related or dependent objects without specifying their concrete classes.”

Source: Design Patterns: Elements of Reusable Object-Oriented Software

In our case, each database family includes:

| Object     | Responsibility            |
| ---------- | ------------------------- |
| Connection | Connect to DB             |
| Command    | Execute SQL               |
| Reader     | Read results              |
| Dialect    | Handle syntax differences |

---

# 5. Architecture Overview

```text id="r5emz0"
                 IDatabaseFactory
                        |
 ------------------------------------------------
 |                      |                       |
SqlServerFactory    MySqlFactory        OracleFactory
 |                      |                       |
Connection         Connection          Connection
Command            Command             Command
Reader             Reader              Reader
Dialect            Dialect             Dialect
```

The client interacts only with abstractions.

---

# 6. Step 1: Create the Abstract Factory

```csharp id="nck45h"
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

This ensures all database providers follow the same contract.

---

# 7. Step 2: Create a Base Factory

Avoid duplication:

```csharp id="ly1p6a"
public abstract class DatabaseFactoryBase : IDatabaseFactory
{
    public string ProviderName { get; }
    public IQueryDialect Dialect { get; }

    protected DatabaseFactoryBase(
        string providerName,
        IQueryDialect dialect)
    {
        ProviderName = providerName;
        Dialect = dialect;
    }

    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbCommand CreateCommand(string sql, IDbConnection connection);
    public abstract IDbDataReader CreateDataReader(IDbCommand command);
}
```

---

# 8. Step 3: Create SQL Dialect Abstraction

Different databases use different SQL syntax.

```csharp id="l9h0nq"
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

# 9. Step 4: Implement Concrete Factory

Example:

```csharp id="abjczp"
public class SqlServerFactory : DatabaseFactoryBase
{
    public SqlServerFactory()
        : base("SQL Server", new SqlServerDialect())
    {
    }

    public override IDbConnection CreateConnection(string connectionString)
        => new SqlServerConnection(connectionString);

    public override IDbCommand CreateCommand(
        string sql,
        IDbConnection connection)
        => new SqlServerCommand(
            sql,
            (SqlServerConnection)connection);

    public override IDbDataReader CreateDataReader(
        IDbCommand command)
        => ((SqlServerCommand)command).ExecuteReader();
}
```

Repeat for:

* MySQL
* Oracle

---

# 10. Step 5: Build the Client

This is where the pattern shines.

```csharp id="4cgb6p"
public class DatabaseClient
{
    private readonly IDatabaseFactory _factory;

    public DatabaseClient(IDatabaseFactory factory)
    {
        _factory = factory;
    }
}
```

The client doesn’t care about concrete implementations.

---

# 11. Step 6: Async Database Operations

```csharp id="8jsg1g"
await conn.OpenAsync();
await cmd.ExecuteNonQueryAsync();
await cmd.ExecuteScalarAsync();
```

Async improves:

* Scalability
* API responsiveness
* Thread utilization

Reference: Microsoft .NET async docs.

[Microsoft Async Programming Guide](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/?utm_source=chatgpt.com)

---

# 12. Step 7: Application Entry Point

This is the composition root.

```csharp id="m2s3a7"
Console.WriteLine("Choose database:");
Console.WriteLine("1. SQL Server");
Console.WriteLine("2. MySQL");
Console.WriteLine("3. Oracle");
```

Then:

```csharp id="9lf7po"
switch(choice)
{
    case "1":
        factory = new SqlServerFactory();
        break;
}
```

This is acceptable because provider selection happens only once.

---

# 13. Running the Application

```csharp id="zwx2h0"
await client.RunQueryAsync(...);
await client.RunNonQueryAsync(...);
await client.RunScalarAsync(...);
```

Example flow:

1. User selects database
2. Factory is created
3. Client receives factory
4. Database operations execute

---

# 14. Adding PostgreSQL

Need PostgreSQL Global Development Group support?

Create:

* PostgreSqlFactory
* PostgreSqlConnection
* PostgreSqlCommand
* PostgreSqlReader
* PostgreSqlDialect

No client changes required.

---

# 15. Benefits

✅ Open/Closed Principle
✅ Easier testing
✅ Better maintainability
✅ Cleaner architecture
✅ Supports multiple providers

---

# 16. Drawbacks

❌ More abstractions
❌ More classes
❌ Overkill for simple apps

Use it only when object families actually exist.

---

# 17. When NOT to Use It

Avoid Abstract Factory when:

* You support only one database
* You need only one object
* Simpler patterns are sufficient

Sometimes this is enough:

```csharp id="c0t74x"
new SqlConnection(connectionString)
```

---

# 18. Real-World Use Cases

Abstract Factory is commonly used in:

* ORM providers
* UI frameworks
* Cloud SDKs
* Cross-platform applications
* Payment gateway integrations

Example: Microsoft [Entity Framework Core Providers](https://learn.microsoft.com/en-us/ef/core/providers/?utm_source=chatgpt.com)

---

# 19. Final Takeaway

Abstract Factory helps you:

* isolate creation logic
* prevent incompatible object combinations
* support future growth

It doesn’t eliminate all conditionals.

It eliminates conditional logic from business workflows.

---

# References

### Books

* Design Patterns: Elements of Reusable Object-Oriented Software — Gamma, Helm, Johnson, Vlissides
* Clean Architecture — Robert C. Martin

---

### Official Documentation

* [Microsoft ADO.NET Documentation](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/?utm_source=chatgpt.com)
* [Microsoft Async Programming Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/?utm_source=chatgpt.com)
* [Oracle Database Documentation](https://www.oracle.com/database/technologies/?utm_source=chatgpt.com)
* [MySQL Documentation](https://dev.mysql.com/doc/?utm_source=chatgpt.com)

---

### Design Principle References

* SOLID Principles by Robert C. Martin
* Dependency Injection in Microsoft .NET:
  [Microsoft Dependency Injection Docs](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection?utm_source=chatgpt.com)

---

This version reads more like a **real educational tutorial**, not just pattern documentation—it teaches *why*, *how*, and *when* to use Abstract Factory while giving readers authoritative references.
