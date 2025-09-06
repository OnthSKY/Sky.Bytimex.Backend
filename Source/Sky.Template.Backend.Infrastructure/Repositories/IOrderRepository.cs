using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Configs.Sales;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IOrderRepository : IRepository<OrderEntity, Guid>
{
    Task<decimal> GetTotalSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
}

public class OrderRepository : Repository<OrderEntity, Guid>, IOrderRepository
{
    public OrderRepository() : base() { }

    public override async Task<(IEnumerable<OrderEntity>, int TotalCount)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var baseSql = "SELECT * FROM sys.orders s WHERE s.is_deleted = FALSE";
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: OrderGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: OrderGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: OrderGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: OrderGridFilterConfig.GetSearchColumns(),
            DbManager.Dialect
        );

        var data = await DbManager.ReadAsync<OrderEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();

        return (data, count?.Count ?? 0);
    }

    public async Task<decimal> GetTotalSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var query = "SELECT COALESCE(SUM(total_amount), 0) FROM sys.orders WHERE order_date BETWEEN @startDate AND @endDate AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<decimal>(query, new Dictionary<string, object> { { "@startDate", startDate }, { "@endDate", endDate } });
        return result.FirstOrDefault();
    }
} 