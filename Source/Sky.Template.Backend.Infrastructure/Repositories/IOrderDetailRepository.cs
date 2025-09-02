using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IOrderDetailRepository : IRepository<OrderDetailEntity, Guid>
{
    Task<List<OrderDetailEntity>> GetByOrderIdAsync(Guid orderId);
}

public class OrderDetailRepository : Repository<OrderDetailEntity, Guid>, IOrderDetailRepository
{
    public OrderDetailRepository() : base() { }

    public async Task<List<OrderDetailEntity>> GetByOrderIdAsync(Guid orderId)
    {
        var sql = "SELECT * FROM sys.orders_details WHERE order_id = @orderId AND is_deleted = FALSE";
        var parameters = new Dictionary<string, object> { { "@orderId", orderId } };
        var data = await DbManager.ReadAsync<OrderDetailEntity>(sql, parameters, GlobalSchema.Name);
        return data.ToList();
    }
}

