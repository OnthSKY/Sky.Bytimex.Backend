namespace Sky.Template.Backend.Contract.Responses.SettingResponses;

public class SettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
}
