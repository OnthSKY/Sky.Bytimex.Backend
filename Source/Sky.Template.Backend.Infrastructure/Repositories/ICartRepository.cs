using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Configs.Sales;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface ICartRepository : IRepository<CartEntity, Guid>
{
    Task<decimal> CalculateTotalPriceAsync(Guid cartId);
    Task<int> GetItemCountAsync(Guid cartId);
}

public class CartRepository : Repository<CartEntity, Guid>, ICartRepository
{
    public CartRepository() : base() { }

    public override async Task<(IEnumerable<CartEntity>, int TotalCount)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var baseSql = "SELECT * FROM sys.carts c WHERE c.is_deleted = FALSE";
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: CartGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: CartGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: CartGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: CartGridFilterConfig.GetSearchColumns(),
            DbManager.Dialect
        );

        var data = await DbManager.ReadAsync<CartEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();

        return (data, count?.Count ?? 0);
    }

    public async Task<decimal> CalculateTotalPriceAsync(Guid cartId)
    {
        var query = "SELECT COALESCE(SUM(price * quantity), 0) FROM sys.cart_items WHERE cart_id = @cartId AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<decimal>(query, new Dictionary<string, object> { { "@cartId", cartId } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<int> GetItemCountAsync(Guid cartId)
    {
        var query = "SELECT COALESCE(SUM(quantity), 0) FROM sys.cart_items WHERE cart_id = @cartId AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<int>(query, new Dictionary<string, object> { { "@cartId", cartId } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }
}
