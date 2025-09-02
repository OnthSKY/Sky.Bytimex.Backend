namespace Sky.Template.Backend.Contract.Responses.ResourceResponses;

public class ResourceListResponse
{
    public List<ResourceDto> Resources { get; set; } = new();
}

public class ResourceDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeleteReason { get; set; }
    public List<ResourcesPermissionDto> Permissions { get; set; }
}

public class ResourcesPermissionDto
{
    public string Code { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}
