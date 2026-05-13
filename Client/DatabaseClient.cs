using AbstractFactoryDemo.Abstractions;

namespace AbstractFactoryDemo.Client;

/// Client — depends only on IDatabaseFactory abstractions.
/// Never references SqlServer*, MySql*, or Oracle* types directly.
public class DatabaseClient(IDatabaseFactory factory)
{
    private readonly IDatabaseFactory _factory = factory;

    public async Task RunQueryAsync(string connectionString, string sql, int limit = 10,
        CancellationToken ct = default)
    {
        string limitedSql = _factory.Dialect.ApplyLimit(sql, limit);
        Console.WriteLine($"\n--- RunQueryAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {limitedSql}");

        IDbConnection conn = _factory.CreateConnection(connectionString);
        await conn.OpenAsync(ct);

        IDbCommand cmd = _factory.CreateCommand(limitedSql, conn);

        IDbDataReader reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            Console.WriteLine(
                $"  TaskItemId={reader.GetValue("TaskItemId"),-4} " +
                $"TaskName={reader.GetValue("TaskName"),-35} " +
                $"IsComplete={reader.GetValue("IsComplete")}");
        }
        await reader.CloseAsync(ct);

        await conn.CloseAsync(ct);
    }

    public async Task RunInsertAsync(string connectionString, string sql,
        string taskName, bool isComplete,
        CancellationToken ct = default)
    {
        Console.WriteLine($"\n--- RunInsertAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {sql}");

        await using IUnitOfWork uow = await _factory.CreateUnitOfWorkAsync(connectionString, ct);
        try
        {
            IDbCommand cmd = uow.CreateCommand(sql);

            cmd.AddParameter(_factory.Dialect.FormatParameter("TaskName"), taskName);
            cmd.AddParameter(_factory.Dialect.FormatParameter("IsComplete"), isComplete);

            int affected = await cmd.ExecuteNonQueryAsync(ct);
            Console.WriteLine($"  Rows affected: {affected}");

            await uow.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Insert failed — rolling back: {ex.Message}");
            await uow.RollbackAsync(ct);
            throw;
        }
    }

    public async Task RunUpdateAsync(string connectionString, string sql,
        int taskItemId, bool isComplete,
        CancellationToken ct = default)
    {
        Console.WriteLine($"\n--- RunUpdateAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {sql}");

        IDbConnection conn = _factory.CreateConnection(connectionString);
        await conn.OpenAsync(ct);

        IDbCommand cmd = _factory.CreateCommand(sql, conn);
        cmd.AddParameter(_factory.Dialect.FormatParameter("IsComplete"), isComplete);
        cmd.AddParameter(_factory.Dialect.FormatParameter("TaskItemId"), taskItemId);

        int affected = await cmd.ExecuteNonQueryAsync(ct);
        Console.WriteLine($"  Rows affected: {affected}");

        await conn.CloseAsync(ct);
    }

    public async Task RunScalarAsync(string connectionString, string sql,
        CancellationToken ct = default)
    {
        Console.WriteLine($"\n--- RunScalarAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {sql}");

        IDbConnection conn = _factory.CreateConnection(connectionString);
        await conn.OpenAsync(ct);

        IDbCommand cmd = _factory.CreateCommand(sql, conn);

        object? result = await cmd.ExecuteScalarAsync(ct);
        Console.WriteLine($"  Scalar result: {result}");

        await conn.CloseAsync(ct);
    }

    /// <summary>
    /// Runs two DML statements atomically via Unit of Work.
    /// Rolls back both if either fails.
    /// </summary>
    public async Task RunUnitOfWorkAsync(
        string connectionString,
        string insertSql, string taskName, bool isComplete,
        string updateSql, int taskItemId, bool updatedIsComplete,
        CancellationToken ct = default)
    {
        Console.WriteLine($"\n--- RunUnitOfWorkAsync via {_factory.ProviderName} ---");

        await using IUnitOfWork uow = await _factory.CreateUnitOfWorkAsync(connectionString, ct);
        try
        {
            IDbCommand insertCmd = uow.CreateCommand(insertSql);
            insertCmd.AddParameter(_factory.Dialect.FormatParameter("TaskName"), taskName);
            insertCmd.AddParameter(_factory.Dialect.FormatParameter("IsComplete"), isComplete);
            int inserted = await insertCmd.ExecuteNonQueryAsync(ct);
            Console.WriteLine($"  [UoW] Insert rows affected: {inserted}");

            IDbCommand updateCmd = uow.CreateCommand(updateSql);
            updateCmd.AddParameter(_factory.Dialect.FormatParameter("IsComplete"), updatedIsComplete);
            updateCmd.AddParameter(_factory.Dialect.FormatParameter("TaskItemId"), taskItemId);
            int updated = await updateCmd.ExecuteNonQueryAsync(ct);
            Console.WriteLine($"  [UoW] Update rows affected: {updated}");

            await uow.CommitAsync(ct);
            Console.WriteLine("  [UoW] Both operations committed atomically.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [UoW] Error — rolling back: {ex.Message}");
            await uow.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Demonstrates calling a stored procedure with Input, Output, and ReturnValue parameters.
    /// </summary>
    public async Task RunStoredProcAsync(
        string connectionString,
        string procSql,
        string filterValue,
        CancellationToken ct = default)
    {
        Console.WriteLine($"\n--- RunStoredProcAsync via {_factory.ProviderName} ---");
        Console.WriteLine($"  SQL: {procSql}");

        IDbConnection conn = _factory.CreateConnection(connectionString);
        await conn.OpenAsync(ct);
        IDbTransaction tx = await conn.BeginTransactionAsync(ct);

        IDbCommand cmd = _factory.CreateCommand(procSql, conn);
        cmd.SetTransaction(tx);

        // Input param with type + size
        IDbParameter filterParam = _factory.CreateParameter(
            _factory.Dialect.FormatParameter("Filter"),
            value: filterValue,
            direction: DbParameterDirection.Input,
            dbType: DbType.String,
            size: 100);
        cmd.AddParameter(filterParam);

        // Output param — value set by DB after execution
        IDbParameter countParam = _factory.CreateParameter(
            _factory.Dialect.FormatParameter("Count"),
            value: null,
            direction: DbParameterDirection.Output,
            dbType: DbType.Int32);
        cmd.AddParameter(countParam);

        // Return value param — proc exit code
        IDbParameter returnParam = _factory.CreateParameter(
            _factory.Dialect.FormatParameter("ReturnCode"),
            value: null,
            direction: DbParameterDirection.ReturnValue,
            dbType: DbType.Int32);
        cmd.AddParameter(returnParam);

        await cmd.ExecuteNonQueryAsync(ct);

        Console.WriteLine($"  Input  '{filterParam.Name}'  = {filterParam.Value}");
        Console.WriteLine($"  Output '{countParam.Name}'   = {cmd.GetParameter(countParam.Name)?.OutputValue}");
        Console.WriteLine($"  Return '{returnParam.Name}'  = {cmd.GetParameter(returnParam.Name)?.OutputValue}");

        await tx.CommitAsync(ct);
        await conn.CloseAsync(ct);
    }
}
