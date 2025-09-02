using Sky.Template.Backend.Core.Enums;

namespace Sky.Template.Backend.Contract.Requests.ReferralRewards;

public class CreateReferralRewardRequest
{
    public Guid ReferredUserId { get; set; }
    public Guid ReferrerUserId { get; set; }
    public string RewardType { get; set; } = string.Empty;
    public decimal? RewardAmount { get; set; }
    public string? RewardCurrency { get; set; }
    public string? RewardDescription { get; set; }
    public string? TriggeredEvent { get; set; }
    public DateTime? RewardExpirationDate { get; set; }
    public string? RewardSource { get; set; }
    public ReferralRewardStatus? RewardStatus { get; set; }
}
