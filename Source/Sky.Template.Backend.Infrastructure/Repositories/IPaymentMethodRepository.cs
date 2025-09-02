using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Configs.System;
using System.Collections.Generic;
using System.Linq;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IPaymentMethodRepository : IRepository<PaymentMethodEntity, Guid>
{
    Task<PaymentMethodEntity?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
}

public class PaymentMethodRepository : Repository<PaymentMethodEntity, Guid>, IPaymentMethodRepository
{
    public PaymentMethodRepository() : base(new GridQueryConfig<PaymentMethodEntity>
    {
        BaseSql = "SELECT * FROM sys.payment_methods pm WHERE pm.is_deleted = FALSE",
        ColumnMappings = PaymentMethodGridFilterConfig.GetColumnMappings(),
        DefaultOrderBy = PaymentMethodGridFilterConfig.GetDefaultOrder(),
        LikeFilterKeys = PaymentMethodGridFilterConfig.GetLikeFilterKeys(),
        SearchColumns = PaymentMethodGridFilterConfig.GetSearchColumns()
    }) { }

    public async Task<PaymentMethodEntity?> GetByCodeAsync(string code)
    {
        var query = "SELECT * FROM sys.payment_methods WHERE code = @code AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<PaymentMethodEntity>(query, new Dictionary<string, object> { { "@code", code } });
        return result.FirstOrDefault();
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var query = excludeId.HasValue
            ? "SELECT COUNT(*) FROM sys.payment_methods WHERE code = @code AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.payment_methods WHERE code = @code AND is_deleted = FALSE";
        var parameters = excludeId.HasValue
            ? new Dictionary<string, object> { { "@code", code }, { "@excludeId", excludeId.Value } }
            : new Dictionary<string, object> { { "@code", code } };
        var count = await DbManager.ReadAsync<DataCountEntity>(query, parameters);
        return count.FirstOrDefault()?.Count == 0;
    }
}
