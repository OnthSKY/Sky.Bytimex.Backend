using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Brand;

[TableName("brand_translations")]
public class BrandTranslationEntity
{
    [DbManager.mColumn("id")]
    public Guid Id { get; set; }

    [DbManager.mColumn("brand_id")]
    public Guid BrandId { get; set; }

    [DbManager.mColumn("language_code")]
    public string LanguageCode { get; set; } = string.Empty;

    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.mColumn("description")]
    public string? Description { get; set; }
}
