using System.Collections.Generic;
using System.Threading.Tasks;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Configs.Sales;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IPaymentRepository : IRepository<PaymentEntity, Guid>
{
    Task<IEnumerable<PaymentEntity>> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<PaymentEntity>> GetByBuyerIdAsync(Guid buyerId);
}

public class PaymentRepository : Repository<PaymentEntity, Guid>, IPaymentRepository
{
    public PaymentRepository() : base(new GridQueryConfig<PaymentEntity>
    {
        BaseSql = "SELECT * FROM sys.payments p WHERE p.is_deleted = FALSE",
        ColumnMappings = PaymentGridFilterConfig.GetColumnMappings(),
        LikeFilterKeys = PaymentGridFilterConfig.GetLikeFilterKeys(),
        SearchColumns = PaymentGridFilterConfig.GetSearchColumns(),
        DefaultOrderBy = PaymentGridFilterConfig.GetDefaultOrder()
    })
    { }

    public async Task<IEnumerable<PaymentEntity>> GetByOrderIdAsync(Guid orderId)
    {
        var sql = "SELECT * FROM sys.payments WHERE order_id = @orderId AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<PaymentEntity>(sql, new Dictionary<string, object> { { "@orderId", orderId } });
        return result;
    }

    public async Task<IEnumerable<PaymentEntity>> GetByBuyerIdAsync(Guid buyerId)
    {
        var sql = "SELECT * FROM sys.payments WHERE buyer_id = @buyerId AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<PaymentEntity>(sql, new Dictionary<string, object> { { "@buyerId", buyerId } });
        return result;
    }
}
