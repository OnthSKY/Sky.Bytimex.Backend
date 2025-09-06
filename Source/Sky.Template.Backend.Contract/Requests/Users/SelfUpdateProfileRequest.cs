using Sky.Template.Backend.Contract.Responses.UserResponses;

namespace Sky.Template.Backend.Contract.Requests.Users;

public class SelfUpdateProfileRequest : BaseRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserPreferencesDto? Preferences { get; set; }
    public NotificationSettingsDto? Notifications { get; set; }
}
