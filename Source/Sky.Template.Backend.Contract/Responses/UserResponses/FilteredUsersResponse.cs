using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.UserResponses;

public class FilteredUsersResponse
{
    public PaginatedData<UserWithRoleDto> Users { get; set; }
}
