

using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    DbConnection Connection { get; }
    DbTransaction Transaction { get; }
    bool IsTransactionStarted { get; }

    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
