using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.RoleResponses;

public class RoleListResponse
{
    public PaginatedData<RoleDto> Roles { get; set; } = new();
} 