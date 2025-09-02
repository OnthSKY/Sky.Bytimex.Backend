namespace Sky.Template.Backend.Contract.Requests.Auth;

public class RegisterRequest
{
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? InviteCode { get; set; }
} 