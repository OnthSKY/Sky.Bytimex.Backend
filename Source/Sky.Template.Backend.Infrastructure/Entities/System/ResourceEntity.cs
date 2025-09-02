using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using System.Text.Json.Serialization;
using Sky.Template.Backend.Contract.Responses.PermissionResponses;

namespace Sky.Template.Backend.Infrastructure.Entities.System;

[TableName("resources")]
public class ResourceEntity : BaseEntity<int>
{
    [DbManager.mColumn("code")]
    public string Code { get; set; } = string.Empty;

    [DbManager.mColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbManager.mColumn("description")]
    public string? Description { get; set; }

    [DbManager.mColumn("status")]
    public string Status { get; set; } = "ACTIVE";

    [DbManager.mColumn("deleted_by")]
    public Guid? DeletedBy { get; set; }

    [DbManager.mColumn("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [DbManager.mColumn("delete_reason")]
    public string? DeleteReason { get; set; }

    [DbManager.mColumn("permissions")]
    public List<ResourcesPermissionEntityDto> Permissions { get; set; } = new();
}

public class ResourcesPermissionEntityDto
{
    [DbManager.mColumn("code")]
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [DbManager.mColumn("action")]
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;
}

