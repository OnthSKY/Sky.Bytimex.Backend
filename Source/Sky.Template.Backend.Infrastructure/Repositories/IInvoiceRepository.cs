using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Configs.Sales;
using Sky.Template.Backend.Infrastructure.Entities;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IInvoiceRepository : IRepository<InvoiceEntity, Guid>
{
    Task<InvoiceEntity?> GetByOrderIdAsync(Guid orderId);
    Task<bool> IsInvoiceNumberUniqueAsync(string invoiceNumber, Guid? excludeId = null);
}

public class InvoiceRepository : Repository<InvoiceEntity, Guid>, IInvoiceRepository
{
    public InvoiceRepository() : base(new GridQueryConfig<InvoiceEntity>
    {
        BaseSql = "SELECT * FROM sys.invoices i WHERE i.is_deleted = FALSE",
        ColumnMappings = InvoiceGridFilterConfig.GetColumnMappings(),
        LikeFilterKeys = InvoiceGridFilterConfig.GetLikeFilterKeys(),
        SearchColumns = InvoiceGridFilterConfig.GetSearchColumns(),
        DefaultOrderBy = InvoiceGridFilterConfig.GetDefaultOrder()
    })
    { }

    public async Task<InvoiceEntity?> GetByOrderIdAsync(Guid orderId)
    {
        var sql = "SELECT * FROM sys.invoices WHERE order_id = @orderId AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<InvoiceEntity>(sql, new Dictionary<string, object> { { "@orderId", orderId } });
        return result.FirstOrDefault();
    }

    public async Task<bool> IsInvoiceNumberUniqueAsync(string invoiceNumber, Guid? excludeId = null)
    {
        var sql = excludeId.HasValue
            ? "SELECT COUNT(*) FROM sys.invoices WHERE invoice_number = @invoiceNumber AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.invoices WHERE invoice_number = @invoiceNumber AND is_deleted = FALSE";
        var parameters = excludeId.HasValue
            ? new Dictionary<string, object> { { "@invoiceNumber", invoiceNumber }, { "@excludeId", excludeId.Value } }
            : new Dictionary<string, object> { { "@invoiceNumber", invoiceNumber } };
        var count = await DbManager.ReadAsync<DataCountEntity>(sql, parameters);
        return count.FirstOrDefault()?.Count == 0;
    }
}
