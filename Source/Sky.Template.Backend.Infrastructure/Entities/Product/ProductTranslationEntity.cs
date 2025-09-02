using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Product;

[TableName("product_translations")]
public class ProductTranslationEntity
{
    [DbManager.mColumn("id")]
    public Guid Id { get; set; }

    [DbManager.mColumn("product_id")]
    public Guid ProductId { get; set; }

    [DbManager.mColumn("language_code")]
    public string LanguageCode { get; set; } = string.Empty;

    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.mColumn("description")]
    public string? Description { get; set; }
}
