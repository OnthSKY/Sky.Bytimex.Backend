using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using System.Collections.Generic;

namespace Sky.Template.Backend.Infrastructure.Entities.Brand;
[Translatable("sys.brand_translations", "brand_id", "language_code", "name", "description")]
[TableName("brands")]
public class BrandEntity : BaseEntity<Guid>
{
    [DbManager.mColumn("code")]
    public string Slug { get; set; } = string.Empty;

    [DbManager.mColumn("logo_url")]
    public string? LogoUrl { get; set; }

    [DbManager.mColumn("status")]
    public string Status { get; set; } = "ACTIVE";

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    public List<BrandTranslationEntity> Translations { get; set; } = new();
}
