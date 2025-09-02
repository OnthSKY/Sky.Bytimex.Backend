namespace Sky.Template.Backend.Contract.Requests.Resources;

public class CreateResourceRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
