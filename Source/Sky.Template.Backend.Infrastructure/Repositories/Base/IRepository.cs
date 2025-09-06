using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using System.Reflection;

namespace Sky.Template.Backend.Infrastructure.Repositories.Base;
public class GridQueryConfig<T>
{
    public Dictionary<string, ColumnMapping> ColumnMappings { get; set; } = new();
    public HashSet<string>? LikeFilterKeys { get; set; }
    public List<string>? SearchColumns { get; set; }
    public string DefaultOrderBy { get; set; } = "created_at DESC";
    public string BaseSql { get; set; } = $"SELECT * FROM {typeof(T).Name.ToLower()}s";
}

public interface IRepository<T, TId> where T : BaseEntity<TId>, new()
{
    #region CRUD Operations
    Task<T?> GetByIdAsync(TId id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<(IEnumerable<T>, int TotalCount)> GetFilteredPaginatedAsync(GridRequest request);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(TId id);
    Task<bool> SoftDeleteAsync(TId id, string reason = "");
    #endregion
}

public class Repository<T, TId> : IRepository<T, TId> where T : BaseEntity<TId>, ISoftDeletable, new()
{
    private readonly string _tableName;
    private readonly string _schemaName;
    private readonly GridQueryConfig<T> _gridQueryConfig;

    public Repository(GridQueryConfig<T>? config = null)
    {
        _schemaName = GlobalSchema.Name;
        _tableName = EntityMetadataHelper.GetTableNameOrThrow<T>();
        _gridQueryConfig = config ?? new GridQueryConfig<T>
        {
            BaseSql = $"SELECT * FROM {_schemaName}.{_tableName}"
        };
    }

    public async Task<T?> GetByIdAsync(TId id)
    {
        var sql = $"SELECT * FROM {_schemaName}.{_tableName} WHERE id = @id AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<T>(sql, new Dictionary<string, object> { { "@id", id } }, _schemaName);
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var sql = $"SELECT * FROM {_schemaName}.{_tableName} WHERE is_deleted = FALSE";
        return await DbManager.ReadAsync<T>(sql, new Dictionary<string, object>(), _schemaName);
    }
    public virtual async Task<(IEnumerable<T>, int TotalCount)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: _gridQueryConfig.BaseSql,
            request: request,
            columnMappings: _gridQueryConfig.ColumnMappings,
            defaultOrderBy: _gridQueryConfig.DefaultOrderBy,
            likeFilterKeys: _gridQueryConfig.LikeFilterKeys,
            searchColumns: _gridQueryConfig.SearchColumns,
            dialect: DbManager.Dialect                    
        );

        var data = await DbManager.ReadAsync<T>(sql, parameters, _schemaName);

        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, _schemaName);

        return (data, count.FirstOrDefault()?.Count ?? 0);
    }

    

    public async Task<T> CreateAsync(T entity)
    {
        //entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsDeleted = false;

        var properties = GetEntityProperties(entity);
        var columns = string.Join(", ", properties.Keys);
        var values = string.Join(", ", properties.Keys.Select(k => "@" + k));

        var sql = $"INSERT INTO {_schemaName}.{_tableName} ({columns}) VALUES ({values}) RETURNING *";

        var result = await DbManager.ReadAsync<T>(sql, properties, _schemaName);
        return result.FirstOrDefault() ?? entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        var properties = GetEntityProperties(entity);
        var setClause = string.Join(", ", properties.Keys.Select(k => $"{k} = @{k}"));

        var sql = $"UPDATE {_schemaName}.{_tableName} SET {setClause} WHERE id = @id AND is_deleted = FALSE RETURNING *";

        var result = await DbManager.ReadAsync<T>(sql, properties, _schemaName);
        return result.FirstOrDefault() ?? entity;
    }

    public async Task<bool> DeleteAsync(TId id)
    {
        var sql = $"DELETE FROM {_schemaName}.{_tableName} WHERE id = @id";
        var parameters = new Dictionary<string, object> { { "@id", id } };

        var affectedRows = await DbManager.ExecuteNonQueryAsync(sql, parameters, _schemaName);
        return affectedRows;
    }

    public async Task<bool> SoftDeleteAsync(TId id, string reason = "")
    {
        var sql = $"UPDATE {_schemaName}.{_tableName} SET is_deleted = TRUE, deleted_at = @deletedAt, deleted_by = @deletedBy, delete_reason = @reason WHERE id = @id";
        var parameters = new Dictionary<string, object>
        {
            { "@id", id },
            { "@deletedAt", DateTime.UtcNow },
            { "@deletedBy", Guid.Empty }, // TODO: Get current user ID from context
            { "@reason", reason }
        };

        var affectedRows = await DbManager.ExecuteNonQueryAsync(sql, parameters, _schemaName);
        return affectedRows;
    }

    private Dictionary<string, object> GetEntityProperties(T entity)
    {
        var properties = new Dictionary<string, object>();
        var type = typeof(T);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.Name == "Id" && prop.GetValue(entity)?.ToString() == "00000000-0000-0000-0000-000000000000")
                continue;

            var value = prop.GetValue(entity);
            if (value != null)
            {
                properties[prop.Name.ToSnakeCase()] = value;
            }
        }

        return properties;
    }
 
}

public static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }
}