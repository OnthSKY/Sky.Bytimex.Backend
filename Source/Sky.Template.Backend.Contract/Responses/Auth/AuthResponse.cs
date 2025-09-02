using System.Text.Json.Serialization;
using Sky.Template.Backend.Core.Models;

namespace Sky.Template.Backend.Contract.Responses.Auth;

public class AuthResponse
{
    public UserForAuth User { get; set; }
    public string Token { get; set; }
    [JsonIgnore]
    public DateTime TokenExpireDate { get; set; }
    [JsonIgnore]
    public string RefreshToken { get; set; }
    [JsonIgnore]
    public DateTime RefreshExpireDate { get; set; }
}

public class UserForAuth : Core.Models.User
{
    public string SchemaName { get; set; }
    public Role Role { get; set; }
}
