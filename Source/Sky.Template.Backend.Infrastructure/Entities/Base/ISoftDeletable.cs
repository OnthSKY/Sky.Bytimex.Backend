namespace Sky.Template.Backend.Infrastructure.Entities.Base;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
    string? DeleteReason { get; set; }
}