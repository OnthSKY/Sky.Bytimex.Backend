using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Sky.Template.Backend.Core.Exceptions;

namespace Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    public DbConnection Connection { get; private set; }
    public DbTransaction Transaction { get; private set; }
    public bool IsTransactionStarted => Transaction != null && Transaction.Connection != null;


    private readonly string _connectionString;
    private readonly DbProviderFactory _factory;

    public UnitOfWork(IConfiguration configuration)
    {
        var providerName = configuration.GetSection("DatabaseConnection:Provider").Value;
        _connectionString = configuration.GetSection("DatabaseConnection:ConnectionString").Value
                            ?? throw new InvalidOperationException("ConnectionStringNotFound");

        _factory = providerName switch
        {
            "PostgreSql" => Npgsql.NpgsqlFactory.Instance,
            "SqlServer" => Microsoft.Data.SqlClient.SqlClientFactory.Instance,
            "MySql" => MySql.Data.MySqlClient.MySqlClientFactory.Instance,
            _ => throw new BusinessRulesException("UnsupportedDatabaseProvider", providerName!)
        };
    }

    public async Task BeginTransactionAsync()
    {
        if (Connection == null)
        {
            Connection = _factory.CreateConnection();
            Connection.ConnectionString = _connectionString;
        }

        if (Connection.State != ConnectionState.Open)
        {
            await Connection.OpenAsync();
        }

        if (Transaction == null || Transaction.Connection == null)
        {
            Transaction = await Connection.BeginTransactionAsync();
        }
    }

    public async Task CommitAsync()
    {
        if (!IsTransactionStarted)
        {
            throw new InvalidOperationException("CommitWithoutTransaction");
        }
        
        Transaction?.Commit();
        await DisposeAsync();
    }

    public async Task RollbackAsync()
    {
        try { Transaction?.Rollback(); }
        catch { /* optional log */ }
        finally { await DisposeAsync(); }
    }

    public async ValueTask DisposeAsync()
    {
        if (Transaction != null)
        {
            await Transaction.DisposeAsync();
            Transaction = null;
        }

        if (Connection != null)
        {
            await Connection.CloseAsync();
            await Connection.DisposeAsync();
            Connection = null;
        }
    }
}
