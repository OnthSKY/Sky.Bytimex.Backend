namespace Sky.Template.Backend.Infrastructure.Entities.Auth;

public class InviteCodeEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string InviteCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid CreatedBy { get; set; }
} 