using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.Roles;

public class RoleFilterRequest : GridRequest
{
    public string? Name { get; set; }
    public string? Status { get; set; }
} 