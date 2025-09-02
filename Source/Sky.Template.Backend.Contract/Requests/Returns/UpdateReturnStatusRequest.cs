using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.Returns;

public class UpdateReturnStatusRequest : BaseRequest
{
    public string Status { get; set; } = string.Empty;
}
