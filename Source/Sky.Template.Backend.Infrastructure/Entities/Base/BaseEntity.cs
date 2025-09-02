using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Base;

public class BaseEntity<TId>
{
    [DbManager.mColumn("id")]
    public TId Id { get; set; }
    [DbManager.mColumn("created_at")]
    public DateTime CreatedAt { get; set; }
    [DbManager.mColumn("created_by")]
    public Guid? CreatedBy { get; set; }
    [DbManager.mColumn("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    [DbManager.mColumn("updated_by")]
    public Guid? UpdatedBy { get; set; }
}