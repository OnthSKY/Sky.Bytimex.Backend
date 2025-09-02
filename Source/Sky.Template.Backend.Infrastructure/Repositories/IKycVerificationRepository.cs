using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Core.Context;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IKycVerificationRepository : IRepository<KycVerificationEntity, Guid>
{
    Task<KycVerificationEntity?> GetByUserIdAsync(Guid userId);
}

public class KycVerificationRepository : Repository<KycVerificationEntity, Guid>, IKycVerificationRepository
{
    public async Task<KycVerificationEntity?> GetByUserIdAsync(Guid userId)
    {
        var sql = "SELECT * FROM sys.kyc_verifications WHERE user_id = @uid";
        var data = await DbManager.ReadAsync<KycVerificationEntity>(sql, new Dictionary<string, object> { {"@uid", userId} }, GlobalSchema.Name);
        return data.FirstOrDefault();
    }
}
