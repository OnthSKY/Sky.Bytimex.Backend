namespace Sky.Template.Backend.Contract.Requests.Resources;

public class UpdateResourceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
