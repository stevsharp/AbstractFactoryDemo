# Abstract Factory Pattern in C#

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
│   ├── IDbConnection.cs
│   ├── IDbCommand.cs
│   ├── IDbDataReader.cs
│   └── IQueryDialect.cs
│
├── SqlServer/
│   ├── SqlServerFactory.cs
│   ├── SqlServerConnection.cs
│   ├── SqlServerCommand.cs
│   ├── SqlServerReader.cs
│   └── SqlServerDialect.cs
│
├── MySql/
│   ├── MySqlFactory.cs
│   ├── MySqlConnection.cs
│   ├── MySqlCommand.cs
│   ├── MySqlReader.cs
│   └── MySqlDialect.cs
│
├── Oracle/
│   ├── OracleFactory.cs
│   ├── OracleConnection.cs
│   ├── OracleCommand.cs
│   ├── OracleReader.cs
│   └── OracleDialect.cs
│
├── Client/
│   └── DatabaseClient.cs
│
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
await client.RunNonQueryAsync(...);
await client.RunScalarAsync(...);
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
