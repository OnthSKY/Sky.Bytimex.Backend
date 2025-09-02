using Sky.Template.Backend.Core.Enums;

namespace Sky.Template.Backend.Contract.Responses.ReferralRewardResponses;

public class ReferralRewardResponse
{
    public Guid Id { get; set; }
    public Guid ReferredUserId { get; set; }
    public Guid ReferrerUserId { get; set; }
    public string RewardType { get; set; } = string.Empty;
    public decimal? RewardAmount { get; set; }
    public string? RewardCurrency { get; set; }
    public string? RewardDescription { get; set; }
    public ReferralRewardStatus RewardStatus { get; set; }
    public string? TriggeredEvent { get; set; }
    public bool IsRewardGranted { get; set; }
    public DateTime? RewardExpirationDate { get; set; }
    public DateTime? GrantedAt { get; set; }
    public Guid? GrantedBy { get; set; }
    public string? RewardSource { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
