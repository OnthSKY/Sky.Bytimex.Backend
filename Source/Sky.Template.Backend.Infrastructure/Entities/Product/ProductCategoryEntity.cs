using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Product;

[TableName("product_categories")]
public class ProductCategoryEntity : BaseEntity<Guid>, ISoftDeletable
{
    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.mColumn("parent_category_id")]
    public Guid? ParentCategoryId { get; set; }

    [DbManager.mColumn("description")]
    public string? Description { get; set; }

    [DbManager.mColumn("is_deleted")]
    public bool IsDeleted { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }
} 