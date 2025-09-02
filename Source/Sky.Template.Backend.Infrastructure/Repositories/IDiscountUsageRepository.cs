using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Configs.Sales;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IDiscountUsageRepository
{
    Task<(IEnumerable<DiscountUsageEntity>, int)> GetFilteredPaginatedAsync(GridRequest request);
    Task<DiscountUsageEntity> CreateAsync(DiscountUsageEntity entity);
    Task<int> CountByDiscountIdAsync(Guid discountId);
    Task<int> CountByDiscountAndBuyerAsync(Guid discountId, Guid buyerId);
}

    public class DiscountUsageRepository : IDiscountUsageRepository
    {
    public async Task<(IEnumerable<DiscountUsageEntity>, int)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var baseSql = "SELECT * FROM sys.discount_usages du";
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: DiscountUsageGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: DiscountUsageGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: DiscountUsageGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: DiscountUsageGridFilterConfig.GetSearchColumns()
        );
        var data = await DbManager.ReadAsync<DiscountUsageEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();
        return (data, count?.Count ?? 0);
    }

    public async Task<DiscountUsageEntity> CreateAsync(DiscountUsageEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        var sql = "INSERT INTO sys.discount_usages (discount_id, buyer_id, order_id, used_at, created_at, created_by) VALUES (@discount_id, @buyer_id, @order_id, @used_at, @created_at, @created_by) RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@discount_id", entity.DiscountId},
            {"@buyer_id", entity.BuyerId},
            {"@order_id", entity.OrderId},
            {"@used_at", entity.UsedAt},
            {"@created_at", entity.CreatedAt},
            {"@created_by", entity.CreatedBy}
        };
        var result = await DbManager.ReadAsync<DiscountUsageEntity>(sql, parameters, GlobalSchema.Name);
        return result.First();
    }

    public async Task<int> CountByDiscountIdAsync(Guid discountId)
    {
        var sql = "SELECT COUNT(1) AS count FROM sys.discount_usages WHERE discount_id = @discount_id";
        var parameters = new Dictionary<string, object> { { "@discount_id", discountId } };
        var result = await DbManager.ReadAsync<DataCountEntity>(sql, parameters, GlobalSchema.Name);
        return result.FirstOrDefault()?.Count ?? 0;
    }

    public async Task<int> CountByDiscountAndBuyerAsync(Guid discountId, Guid buyerId)
    {
        var sql = "SELECT COUNT(1) AS count FROM sys.discount_usages WHERE discount_id = @discount_id AND buyer_id = @buyer_id";
        var parameters = new Dictionary<string, object> { { "@discount_id", discountId }, { "@buyer_id", buyerId } };
        var result = await DbManager.ReadAsync<DataCountEntity>(sql, parameters, GlobalSchema.Name);
        return result.FirstOrDefault()?.Count ?? 0;
    }
}
