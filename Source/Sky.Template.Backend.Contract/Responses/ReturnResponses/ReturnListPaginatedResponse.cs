using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ReturnResponses;

public class ReturnListPaginatedResponse
{
    public PaginatedData<ReturnResponse> Returns { get; set; } = new();
}

