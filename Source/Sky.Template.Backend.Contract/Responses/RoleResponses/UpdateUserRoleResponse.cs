namespace Sky.Template.Backend.Contract.Responses.RoleResponses;

public class UpdateUserRoleResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public DateTime UpdatedAt { get; set; }

}
