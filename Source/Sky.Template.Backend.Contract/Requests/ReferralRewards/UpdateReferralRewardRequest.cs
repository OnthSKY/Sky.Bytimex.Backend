using Sky.Template.Backend.Core.Enums;

namespace Sky.Template.Backend.Contract.Requests.ReferralRewards;

public class UpdateReferralRewardRequest
{
    public ReferralRewardStatus RewardStatus { get; set; }
    public decimal? RewardAmount { get; set; }
    public string? RewardCurrency { get; set; }
    public string? RewardDescription { get; set; }
    public bool? IsRewardGranted { get; set; }
    public DateTime? RewardExpirationDate { get; set; }
    public DateTime? GrantedAt { get; set; }
    public Guid? GrantedBy { get; set; }
    public string? RewardSource { get; set; }
}
