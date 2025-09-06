using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Configs.Sales;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IDiscountRepository : IRepository<DiscountEntity, Guid>
{
    Task<DiscountEntity?> GetByCodeAsync(string code);
}

public class DiscountRepository : Repository<DiscountEntity, Guid>, IDiscountRepository
{
    public override async Task<(IEnumerable<DiscountEntity>, int)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var baseSql = "SELECT * FROM sys.discounts d WHERE d.is_deleted = FALSE";
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: DiscountGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: DiscountGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: DiscountGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: DiscountGridFilterConfig.GetSearchColumns(),
            DbManager.Dialect
        );

        var data = await DbManager.ReadAsync<DiscountEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();

        return (data, count?.Count ?? 0);
    }

    public async Task<DiscountEntity?> GetByCodeAsync(string code)
    {
        var sql = "SELECT * FROM sys.discounts WHERE code = @code AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<DiscountEntity>(sql, new Dictionary<string, object> { { "@code", code } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }
}
