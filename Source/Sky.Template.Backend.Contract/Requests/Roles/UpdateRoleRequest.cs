namespace Sky.Template.Backend.Contract.Requests.Roles;

public class UpdateRoleRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "ACTIVE";
} 