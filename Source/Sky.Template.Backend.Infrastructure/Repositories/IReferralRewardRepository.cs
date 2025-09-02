using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Core.Context;
using System.Collections.Generic;
using System.Data.Common;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IReferralRewardRepository : IRepository<ReferralRewardEntity, Guid>
{
    Task<IEnumerable<ReferralRewardEntity>> GetListAsync(Guid? referrerUserId, Guid? referredUserId, string? status);
    Task CreateAsync(ReferralRewardEntity entity, DbConnection connection, DbTransaction transaction);
}

public class ReferralRewardRepository : Repository<ReferralRewardEntity, Guid>, IReferralRewardRepository
{
    public async Task<IEnumerable<ReferralRewardEntity>> GetListAsync(Guid? referrerUserId, Guid? referredUserId, string? status)
    {
        var sql = $"SELECT * FROM {GlobalSchema.Name}.referral_rewards WHERE is_deleted = FALSE";
        var parameters = new Dictionary<string, object>();
        if (referrerUserId.HasValue)
        {
            sql += " AND referrer_user_id = @referrerUserId";
            parameters["@referrerUserId"] = referrerUserId.Value;
        }
        if (referredUserId.HasValue)
        {
            sql += " AND referred_user_id = @referredUserId";
            parameters["@referredUserId"] = referredUserId.Value;
        }
        if (!string.IsNullOrEmpty(status))
        {
            sql += " AND reward_status = @status";
            parameters["@status"] = status;
        }
        return await DbManager.ReadAsync<ReferralRewardEntity>(sql, parameters, GlobalSchema.Name);
    }

    public async Task CreateAsync(ReferralRewardEntity entity, DbConnection connection, DbTransaction transaction)
    {
        var sql = $"INSERT INTO {GlobalSchema.Name}.referral_rewards (id, referred_user_id, referrer_user_id, reward_status, triggered_event, is_deleted, created_at) VALUES (@id, @referred_user_id, @referrer_user_id, @reward_status, @triggered_event, FALSE, @created_at)";
        var parameters = new Dictionary<string, object>
        {
            {"@id", entity.Id},
            {"@referred_user_id", entity.ReferredUserId},
            {"@referrer_user_id", entity.ReferrerUserId},
            {"@reward_status", entity.RewardStatus},
            {"@triggered_event", entity.TriggeredEvent},
            {"@created_at", entity.CreatedAt}
        };
        await DbManager.ExecuteTransactionNonQueryWithAsync(sql, parameters, connection, transaction, GlobalSchema.Name);
    }
}
