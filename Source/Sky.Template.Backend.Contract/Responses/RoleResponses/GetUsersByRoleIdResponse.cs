
using Sky.Template.Backend.Core.Models;
using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.RoleResponses;

public class GetUsersByRoleIdResponse
{
    public PaginatedData<Core.Models.User> Users { get; set; }
}

