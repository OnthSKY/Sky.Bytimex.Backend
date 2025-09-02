namespace Sky.Template.Backend.Contract.Responses.PublicUsers;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PreferredLanguage { get; set; }
}
