using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.Permissions;

public class PermissionFilterRequest : GridRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
} 