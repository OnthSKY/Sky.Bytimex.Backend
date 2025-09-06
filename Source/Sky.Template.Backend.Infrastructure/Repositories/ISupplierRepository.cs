using Sky.Template.Backend.Infrastructure.Entities.Supplier;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Configs.Suppliers;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Repositories.Base;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface ISupplierRepository : IRepository<SupplierEntity, Guid>
{
    Task<SupplierEntity?> GetSupplierByEmailAsync(string email);
    Task<SupplierEntity?> GetSupplierByTaxNumberAsync(string taxNumber);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<bool> IsTaxNumberUniqueAsync(string taxNumber, Guid? excludeId = null);
}

public class SupplierRepository : Repository<SupplierEntity, Guid>, ISupplierRepository
{
    public override async Task<(IEnumerable<SupplierEntity>, int TotalCount)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var baseSql = "SELECT * FROM sys.suppliers s WHERE s.is_deleted = FALSE";
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: SupplierGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: SupplierGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: SupplierGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: SupplierGridFilterConfig.GetSearchColumns(),
            DbManager.Dialect
        );

        var data = await DbManager.ReadAsync<SupplierEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();
        
        return (data, count?.Count ?? 0);
    }

    public async Task<SupplierEntity?> GetSupplierByEmailAsync(string email)
    {
        var sql = "SELECT * FROM sys.suppliers WHERE email = @email AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<SupplierEntity>(sql, new Dictionary<string, object> { { "@email", email } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<SupplierEntity?> GetSupplierByTaxNumberAsync(string taxNumber)
    {
        var sql = "SELECT * FROM sys.suppliers WHERE tax_number = @taxNumber AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<SupplierEntity>(sql, new Dictionary<string, object> { { "@taxNumber", taxNumber } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        var sql = excludeId.HasValue 
            ? "SELECT COUNT(*) FROM sys.suppliers WHERE email = @email AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.suppliers WHERE email = @email AND is_deleted = FALSE";
        
        var parameters = new Dictionary<string, object> { { "@email", email } };
        if (excludeId.HasValue)
            parameters.Add("@excludeId", excludeId.Value);

        var count = await DbManager.ReadAsync<DataCountEntity>(sql, parameters, GlobalSchema.Name);
        return count.FirstOrDefault()?.Count == 0;
    }

    public async Task<bool> IsTaxNumberUniqueAsync(string taxNumber, Guid? excludeId = null)
    {
        var sql = excludeId.HasValue 
            ? "SELECT COUNT(*) FROM sys.suppliers WHERE tax_number = @taxNumber AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.suppliers WHERE tax_number = @taxNumber AND is_deleted = FALSE";
        
        var parameters = new Dictionary<string, object> { { "@taxNumber", taxNumber } };
        if (excludeId.HasValue)
            parameters.Add("@excludeId", excludeId.Value);

        var count = await DbManager.ReadAsync<DataCountEntity>(sql, parameters, GlobalSchema.Name);
        return count.FirstOrDefault()?.Count == 0;
    }
} 