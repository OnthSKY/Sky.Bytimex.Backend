namespace Sky.Template.Backend.Contract.Requests.AdminUsers;

public class SelfUpdateProfileRequest : BaseRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PreferredLanguage { get; set; }
}
