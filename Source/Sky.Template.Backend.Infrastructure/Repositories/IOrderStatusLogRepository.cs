using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IOrderStatusLogRepository
{
    Task<OrderStatusLogEntity> CreateAsync(OrderStatusLogEntity entity);
    Task<IEnumerable<OrderStatusLogEntity>> GetByOrderIdAsync(Guid orderId);
}

public class OrderStatusLogRepository : IOrderStatusLogRepository
{
    private const string Table = "sys.order_status_logs";

    public async Task<OrderStatusLogEntity> CreateAsync(OrderStatusLogEntity entity)
    {
        const string sql = $"INSERT INTO {Table} (id, order_id, old_status, new_status, changed_by, changed_at, note) " +
                           "VALUES (@id, @order_id, @old_status, @new_status, @changed_by, @changed_at, @note) RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@id", entity.Id},
            {"@order_id", entity.OrderId},
            {"@old_status", entity.OldStatus},
            {"@new_status", entity.NewStatus},
            {"@changed_by", entity.ChangedBy},
            {"@changed_at", entity.ChangedAt},
            {"@note", entity.Note ?? string.Empty}
        };
        var result = await DbManager.ReadAsync<OrderStatusLogEntity>(sql, parameters, GlobalSchema.Name);
        return result.First();
    }

    public async Task<IEnumerable<OrderStatusLogEntity>> GetByOrderIdAsync(Guid orderId)
    {
        var sql = $"SELECT * FROM {Table} WHERE order_id = @order_id ORDER BY changed_at";
        var parameters = new Dictionary<string, object> { {"@order_id", orderId} };
        var result = await DbManager.ReadAsync<OrderStatusLogEntity>(sql, parameters, GlobalSchema.Name);
        return result;
    }
}
