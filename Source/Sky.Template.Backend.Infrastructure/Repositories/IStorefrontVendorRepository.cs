using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IStorefrontVendorRepository
{
    Task<(IEnumerable<VendorWithProductCountEntity> Vendors, int TotalCount)> GetVendorsAsync(GridRequest request);
    Task<VendorEntity?> GetActiveBySlugAsync(string slug);
    Task<VendorEntity?> GetActiveByIdAsync(Guid id);
}

public class StorefrontVendorRepository : IStorefrontVendorRepository
{
    private readonly GridQueryConfig<VendorWithProductCountEntity> _config = new()
    {
        BaseSql = "SELECT v.*, (SELECT COUNT(1) FROM sys.products p WHERE p.vendor_id = v.id AND p.status = 'ACTIVE') AS product_count FROM sys.vendors v WHERE v.is_deleted = FALSE",
        DefaultOrderBy = "v.created_at DESC",
        ColumnMappings = new Dictionary<string, ColumnMapping>(StringComparer.OrdinalIgnoreCase)
        {
            { "id",           new ColumnMapping("v.id",          typeof(Guid)) },
            { "name",         new ColumnMapping("v.name",        typeof(string)) },
            { "slug",         new ColumnMapping("v.slug",        typeof(string)) },
            { "status",       new ColumnMapping("v.status",      typeof(string)) },
            { "created_at",   new ColumnMapping("v.created_at",  typeof(DateTime)) },
            { "product_count",new ColumnMapping("product_count", typeof(int)) }
        },
        LikeFilterKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "name", "slug" },
        SearchColumns = new List<string>{ "v.name", "v.slug", "v.short_description" }
    };

    internal (string Sql, Dictionary<string, object> Params) BuildListSql(GridRequest request)
    {
        request.Filters ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!request.Filters.ContainsKey("status"))
            request.Filters["status"] = "ACTIVE";
        if (request.Filters.TryGetValue("slug", out var slug))
            request.Filters["slug"] = slug.ToLowerInvariant();

        var (sql, parameters) = GridQueryBuilder.Build(
            _config.BaseSql,
            request,
            _config.ColumnMappings,
            _config.DefaultOrderBy,
            _config.LikeFilterKeys,
            _config.SearchColumns,
            DbManager.Dialect
        );
        return (sql, parameters);
    }

    public async Task<(IEnumerable<VendorWithProductCountEntity> Vendors, int TotalCount)> GetVendorsAsync(GridRequest request)
    {
        var (sql, parameters) = BuildListSql(request);
        var data = await DbManager.ReadAsync<VendorWithProductCountEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault()?.Count ?? 0;
        return (data, count);
    }

    public async Task<VendorEntity?> GetActiveBySlugAsync(string slug)
    {
        const string sql = "SELECT * FROM sys.vendors v WHERE v.is_deleted = FALSE AND v.status = 'ACTIVE' AND lower(v.slug) = lower(@slug) LIMIT 1";
        var result = await DbManager.ReadAsync<VendorEntity>(sql, new Dictionary<string, object> { { "@slug", slug } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<VendorEntity?> GetActiveByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM sys.vendors v WHERE v.is_deleted = FALSE AND v.status = 'ACTIVE' AND v.id = @id LIMIT 1";
        var result = await DbManager.ReadAsync<VendorEntity>(sql, new Dictionary<string, object> { { "@id", id } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }
}
