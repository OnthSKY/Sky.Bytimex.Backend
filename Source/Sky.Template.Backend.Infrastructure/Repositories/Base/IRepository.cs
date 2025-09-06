using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;

namespace Sky.Template.Backend.Infrastructure.Repositories.Base;
public class GridQueryConfig<T>
{
    public Dictionary<string, ColumnMapping> ColumnMappings { get; set; } = new();
    public HashSet<string>? LikeFilterKeys { get; set; }
    public List<string>? SearchColumns { get; set; }
    public string DefaultOrderBy { get; set; } = "created_at DESC";
    public string BaseSql { get; set; } = $"SELECT * FROM {typeof(T).Name.ToLower()}s";

    /// <summary>Optional translation configuration for this entity.</summary>
    public TranslationConfig? Translation { get; set; }
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
    private readonly ILanguageResolver _langResolver;

    public Repository(ILanguageResolver? langResolver = null, GridQueryConfig<T>? config = null)
    {
        _langResolver = langResolver ??
                        ServiceTool.ServiceProvider?.GetService<ILanguageResolver>() ??
                        new DefaultLanguageResolver();

        _schemaName = GlobalSchema.Name;
        _tableName = EntityMetadataHelper.GetTableNameOrThrow<T>();
        _gridQueryConfig = config ?? new GridQueryConfig<T>
        {
            BaseSql = $"SELECT * FROM {_schemaName}.{_tableName} t"
        };

        var translatableAttr = typeof(T).GetCustomAttribute<TranslatableAttribute>();
        if (translatableAttr != null)
        {
            _gridQueryConfig.Translation = new TranslationConfig
            {
                TranslationTable = translatableAttr.TranslationTable,
                ForeignKeyColumn = translatableAttr.ForeignKeyColumn,
                LanguageColumn = translatableAttr.LanguageColumn,
                MainAlias = "t",
                ProjectedColumns = translatableAttr.ProjectedColumns.Select(c => new TranslationColumn(c)).ToArray()
            };
        }
    }

    private (string sql, Dictionary<string, object> parameters) AttachLanguage(
        string sql,
        Dictionary<string, object>? parameters = null)
    {
        parameters ??= new Dictionary<string, object>();
        if (_gridQueryConfig.Translation is not null)
        {
            parameters["@lang"] = _langResolver.GetLanguageOrDefault();
        }
        return (sql, parameters);
    }

    private string WrapWithTranslationsIfConfigured(string baseSql)
    {
        var cfg = _gridQueryConfig.Translation;
        if (cfg is null) return baseSql;

        var (joins, projection) = TranslationSqlBuilder.Build(cfg);
        if (string.IsNullOrWhiteSpace(joins) || string.IsNullOrWhiteSpace(projection))
            return baseSql;

        var idxFrom = baseSql.IndexOf(" FROM ", StringComparison.OrdinalIgnoreCase);
        if (idxFrom < 0) return baseSql;

        var selectPart = baseSql[..idxFrom];
        var fromPart = baseSql[idxFrom..];

        var newSelect = selectPart.TrimEnd() + ",\n       " + projection + "\n";
        var wrapped = newSelect + fromPart + "\n" + joins;
        return wrapped;
    }

    private sealed class DefaultLanguageResolver : ILanguageResolver
    {
        public string GetLanguageOrDefault() => "en";
    }

    public async Task<T?> GetByIdAsync(TId id)
    {
        var baseSql = $"SELECT * FROM {_schemaName}.{_tableName} t WHERE t.id = @id AND t.is_deleted = FALSE";
        baseSql = WrapWithTranslationsIfConfigured(baseSql);
        var (sql, parameters) = AttachLanguage(baseSql, new Dictionary<string, object> { { "@id", id! } });
        var result = await DbManager.ReadAsync<T>(sql, parameters, _schemaName);
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var baseSql = $"SELECT * FROM {_schemaName}.{_tableName} t WHERE t.is_deleted = FALSE";
        baseSql = WrapWithTranslationsIfConfigured(baseSql);
        var (sql, parameters) = AttachLanguage(baseSql);
        return await DbManager.ReadAsync<T>(sql, parameters, _schemaName);
    }
    public virtual async Task<(IEnumerable<T>, int TotalCount)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var baseSql = WrapWithTranslationsIfConfigured(_gridQueryConfig.BaseSql);

        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: _gridQueryConfig.ColumnMappings,
            defaultOrderBy: _gridQueryConfig.DefaultOrderBy,
            likeFilterKeys: _gridQueryConfig.LikeFilterKeys,
            searchColumns: _gridQueryConfig.SearchColumns,
            dialect: DbManager.Dialect
        );
        (sql, parameters) = AttachLanguage(sql, parameters);

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