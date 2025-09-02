namespace Sky.Template.Backend.Core.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PermissionNamesRaw { get; set; }
    public List<string> PermissionList => PermissionNamesRaw?.Split(',').Select(p => p.Trim()).ToList() ?? new();
}
