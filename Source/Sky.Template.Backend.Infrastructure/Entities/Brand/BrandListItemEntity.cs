using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using System;
using Sky.Template.Backend.Core.Attributes;

namespace Sky.Template.Backend.Infrastructure.Entities.Brand;

[TableName("brands")]
public class BrandListItemEntity
{
    [DbManager.mColumn("id")]
    public Guid Id { get; set; }

    [DbManager.mColumn("slug")]
    public string Slug { get; set; } = string.Empty;

    [DbManager.mColumn("logo_url")]
    public string? LogoUrl { get; set; }

    [DbManager.mColumn("status")]
    public string Status { get; set; } = string.Empty;

    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;
}
