using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.ErrorLogResponses;

public class ErrorLogListPaginatedResponse
{
    public PaginatedData<ErrorLogResponse> Logs { get; set; } = new();
}
