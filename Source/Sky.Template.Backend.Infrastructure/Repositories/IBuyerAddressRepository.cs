using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IBuyerAddressRepository : IRepository<BuyerAddressEntity, Guid>
{
    Task<IEnumerable<BuyerAddressEntity>> GetByBuyerIdAsync(Guid buyerId);
    Task ClearDefaultAsync(Guid buyerId);
    Task<bool> HasDefaultAsync(Guid buyerId, Guid? excludeId = null);
}

public class BuyerAddressRepository : Repository<BuyerAddressEntity, Guid>, IBuyerAddressRepository
{
    public BuyerAddressRepository() : base(new GridQueryConfig<BuyerAddressEntity>
    {
        BaseSql = "SELECT * FROM sys.buyer_addresses WHERE is_deleted = FALSE",
        ColumnMappings = new Dictionary<string, string>
        {
            { "city", "city" },
            { "country", "country" },
            { "postal_code", "postal_code" }
        },
        LikeFilterKeys = new HashSet<string> { "city", "country", "postal_code" },
        SearchColumns = new List<string> { "city", "country", "postal_code" },
        DefaultOrderBy = "created_at DESC"
    })
    { }

    public async Task<IEnumerable<BuyerAddressEntity>> GetByBuyerIdAsync(Guid buyerId)
    {
        var sql = "SELECT * FROM sys.buyer_addresses WHERE buyer_id = @buyerId AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<BuyerAddressEntity>(sql, new Dictionary<string, object>
        {
            { "@buyerId", buyerId }
        });
        return result;
    }

    public async Task ClearDefaultAsync(Guid buyerId)
    {
        var sql = "UPDATE sys.buyer_addresses SET is_default = FALSE WHERE buyer_id = @buyerId AND is_deleted = FALSE";
        await DbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>
        {
            { "@buyerId", buyerId }
        });
    }

    public async Task<bool> HasDefaultAsync(Guid buyerId, Guid? excludeId = null)
    {
        var sql = excludeId.HasValue
            ? "SELECT COUNT(*) FROM sys.buyer_addresses WHERE buyer_id = @buyerId AND is_default = TRUE AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.buyer_addresses WHERE buyer_id = @buyerId AND is_default = TRUE AND is_deleted = FALSE";
        var parameters = excludeId.HasValue
            ? new Dictionary<string, object> { { "@buyerId", buyerId }, { "@excludeId", excludeId.Value } }
            : new Dictionary<string, object> { { "@buyerId", buyerId } };
        var count = await DbManager.ReadAsync<DataCountEntity>(sql, parameters);
        return (count.FirstOrDefault()?.Count ?? 0) > 0;
    }
}
