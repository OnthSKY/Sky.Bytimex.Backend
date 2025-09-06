using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Configs.ErrorLogs;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.ErrorLog;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IErrorLogRepository
{
    Task<ErrorLogEntity> InsertAsync(ErrorLogEntity entity);
    Task<(IEnumerable<ErrorLogEntity>, int TotalCount)> GetAllAsync(GridRequest request);
    Task<ErrorLogEntity?> GetByIdAsync(Guid id);
}

public class ErrorLogRepository : IErrorLogRepository
{
    private const string Table = "sys.error_logs";

    public async Task<ErrorLogEntity> InsertAsync(ErrorLogEntity entity)
    {
        const string sql = $"INSERT INTO {Table} (id, message, stack_trace, source, path, method, created_at) " +
                           "VALUES (@id, @message, @stack_trace, @source, @path, @method, @created_at) RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@id", entity.Id},
            {"@message", entity.Message},
            {"@stack_trace", entity.StackTrace ?? string.Empty},
            {"@source", entity.Source ?? string.Empty},
            {"@path", entity.Path ?? string.Empty},
            {"@method", entity.Method ?? string.Empty},
            {"@created_at", entity.CreatedAt}
        };
        var result = await DbManager.ReadAsync<ErrorLogEntity>(sql, parameters, GlobalSchema.Name);
        return result.First();
    }

    public async Task<(IEnumerable<ErrorLogEntity>, int TotalCount)> GetAllAsync(GridRequest request)
    {
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: $"SELECT * FROM {Table} el",
            request: request,
            columnMappings: ErrorLogGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: ErrorLogGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: ErrorLogGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: ErrorLogGridFilterConfig.GetSearchColumns(),
            DbManager.Dialect
        );
        var data = await DbManager.ReadAsync<ErrorLogEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();
        return (data, count?.Count ?? 0);
    }

    public async Task<ErrorLogEntity?> GetByIdAsync(Guid id)
    {
        var sql = $"SELECT * FROM {Table} WHERE id = @id";
        var result = await DbManager.ReadAsync<ErrorLogEntity>(sql, new Dictionary<string, object>{{"@id", id}}, GlobalSchema.Name);
        return result.FirstOrDefault();
    }
}
