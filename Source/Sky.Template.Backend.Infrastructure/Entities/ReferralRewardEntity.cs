using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities;

[TableName("referral_rewards")]
public class ReferralRewardEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("referred_user_id")]
    public Guid ReferredUserId { get; set; }

    [DbManager.mColumn("referrer_user_id")]
    public Guid ReferrerUserId { get; set; }

    [DbManager.mColumn("reward_type")]
    public string RewardType { get; set; } = string.Empty;

    [DbManager.mColumn("reward_amount")]
    public decimal? RewardAmount { get; set; }

    [DbManager.mColumn("reward_currency")]
    public string? RewardCurrency { get; set; }

    [DbManager.mColumn("reward_description")]
    public string? RewardDescription { get; set; }

    [DbManager.mColumn("reward_status")]
    public string RewardStatus { get; set; } = string.Empty;

    [DbManager.mColumn("triggered_event")]
    public string? TriggeredEvent { get; set; }

    [DbManager.mColumn("is_reward_granted")]
    public bool IsRewardGranted { get; set; }

    [DbManager.mColumn("reward_expiration_date")]
    public DateTime? RewardExpirationDate { get; set; }

    [DbManager.mColumn("granted_at")]
    public DateTime? GrantedAt { get; set; }

    [DbManager.mColumn("granted_by")]
    public Guid? GrantedBy { get; set; }

    [DbManager.mColumn("reward_source")]
    public string? RewardSource { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
}
