namespace Sky.Template.Backend.Contract.Requests.Auth;

public class RegisterUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PreferredLanguage { get; set; }
    public string? ReferralCode { get; set; }
}

