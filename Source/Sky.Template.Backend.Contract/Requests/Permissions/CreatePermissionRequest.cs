namespace Sky.Template.Backend.Contract.Requests.Permissions;

public class CreatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
} 