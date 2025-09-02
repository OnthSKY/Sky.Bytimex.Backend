using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Configs.Sales;
using System.Linq;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IShipmentRepository : IRepository<ShipmentEntity, Guid>
{
    Task<IEnumerable<ShipmentEntity>> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<ShipmentEntity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ShipmentEntity>> GetByOrderIdAndBuyerIdAsync(Guid orderId, Guid buyerId);
    Task<ShipmentEntity?> GetByTrackingNumberAndBuyerIdAsync(string trackingNumber, Guid buyerId);
}

public class ShipmentRepository : Repository<ShipmentEntity, Guid>, IShipmentRepository
{
    public ShipmentRepository() : base(new GridQueryConfig<ShipmentEntity>
    {
        BaseSql = "SELECT * FROM sys.shipments s WHERE s.is_deleted = FALSE",
        ColumnMappings = ShipmentGridFilterConfig.GetColumnMappings(),
        LikeFilterKeys = ShipmentGridFilterConfig.GetLikeFilterKeys(),
        SearchColumns = ShipmentGridFilterConfig.GetSearchColumns(),
        DefaultOrderBy = ShipmentGridFilterConfig.GetDefaultOrder()
    })
    { }

    public async Task<IEnumerable<ShipmentEntity>> GetByOrderIdAsync(Guid orderId)
    {
        var sql = "SELECT * FROM sys.shipments WHERE order_id = @orderId AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<ShipmentEntity>(sql, new Dictionary<string, object> { { "@orderId", orderId } });
        return result;
    }

    public async Task<IEnumerable<ShipmentEntity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var sql = "SELECT * FROM sys.shipments WHERE shipment_date BETWEEN @startDate AND @endDate AND is_deleted = FALSE";
        var parameters = new Dictionary<string, object>
        {
            {"@startDate", startDate},
            {"@endDate", endDate}
        };
        var result = await DbManager.ReadAsync<ShipmentEntity>(sql, parameters);
        return result;
    }

    public async Task<IEnumerable<ShipmentEntity>> GetByOrderIdAndBuyerIdAsync(Guid orderId, Guid buyerId)
    {
        var sql = "SELECT s.* FROM sys.shipments s JOIN sys.orders o ON s.order_id = o.id WHERE s.order_id = @orderId AND o.buyer_id = @buyerId AND s.is_deleted = FALSE";
        var parameters = new Dictionary<string, object>
        {
            {"@orderId", orderId},
            {"@buyerId", buyerId}
        };
        var result = await DbManager.ReadAsync<ShipmentEntity>(sql, parameters);
        return result;
    }

    public async Task<ShipmentEntity?> GetByTrackingNumberAndBuyerIdAsync(string trackingNumber, Guid buyerId)
    {
        var sql = "SELECT s.* FROM sys.shipments s JOIN sys.orders o ON s.order_id = o.id WHERE s.tracking_number = @trackingNumber AND o.buyer_id = @buyerId AND s.is_deleted = FALSE LIMIT 1";
        var parameters = new Dictionary<string, object>
        {
            {"@trackingNumber", trackingNumber},
            {"@buyerId", buyerId}
        };
        var result = await DbManager.ReadAsync<ShipmentEntity>(sql, parameters);
        return result.FirstOrDefault();
    }
}
