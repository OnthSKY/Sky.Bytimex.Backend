using Sky.Template.Backend.Core.Models;

namespace Sky.Template.Backend.Contract.Responses.UserResponses;

public class SingleUserResponse
{
    public UserWithRoleDto User { get; set; } 
}

public class UserWithRoleDto : Core.Models.User
{
    public Role Role { get; set; }
}
