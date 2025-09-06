using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Repositories.Base;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IBuyerRepository : IRepository<BuyerEntity, Guid>
{
    Task<BuyerEntity?> GetBuyerByEmailAsync(string email);
    Task<BuyerEntity?> GetBuyerByPhoneAsync(string phone);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeId = null);
}

public class BuyerRepository : Repository<BuyerEntity, Guid>, IBuyerRepository
{
    public BuyerRepository() : base(new GridQueryConfig<BuyerEntity>
    {
        BaseSql = "SELECT * FROM sys.buyers WHERE is_deleted = FALSE",
        ColumnMappings = new Dictionary<string, ColumnMapping>
        {
            { "name",    new ColumnMapping("name",    typeof(string)) },
            { "surname", new ColumnMapping("surname", typeof(string)) },
            { "email",   new ColumnMapping("email",   typeof(string)) },
            { "phone",   new ColumnMapping("phone",   typeof(string)) }
        },
        LikeFilterKeys = new HashSet<string> { "name", "surname", "email", "phone" },
        SearchColumns = new List<string> { "name", "surname", "email", "phone" },
        DefaultOrderBy = "created_at DESC"
    })
    { }

    public async Task<BuyerEntity?> GetBuyerByEmailAsync(string email)
    {
        var query = "SELECT * FROM sys.buyers WHERE email = @email AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<BuyerEntity>(query, new Dictionary<string, object> { { "@email", email } });
        return result.FirstOrDefault();
    }

    public async Task<BuyerEntity?> GetBuyerByPhoneAsync(string phone)
    {
        var query = "SELECT * FROM sys.buyers WHERE phone = @phone AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<BuyerEntity>(query, new Dictionary<string, object> { { "@phone", phone } });
        return result.FirstOrDefault();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        var query = excludeId.HasValue
            ? "SELECT COUNT(*) FROM sys.buyers WHERE email = @email AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.buyers WHERE email = @email AND is_deleted = FALSE";

        var parameters = excludeId.HasValue
            ? new Dictionary<string, object> { { "@email", email }, { "@excludeId", excludeId.Value } }
            : new Dictionary<string, object> { { "@email", email } };

        var count = await DbManager.ReadAsync<DataCountEntity>(query, parameters);
        return count.FirstOrDefault()?.Count == 0;
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeId = null)
    {
        var query = excludeId.HasValue
            ? "SELECT COUNT(*) FROM sys.buyers WHERE phone = @phone AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.buyers WHERE phone = @phone AND is_deleted = FALSE";

        var parameters = excludeId.HasValue
            ? new Dictionary<string, object> { { "@phone", phone }, { "@excludeId", excludeId.Value } }
            : new Dictionary<string, object> { { "@phone", phone } };

        var count = await DbManager.ReadAsync<DataCountEntity>(query, parameters);
        return count.FirstOrDefault()?.Count == 0;
    }
} 