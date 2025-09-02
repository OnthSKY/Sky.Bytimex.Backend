using Sky.Template.Backend.Contract.Requests;
using System.Collections.Generic;

namespace Sky.Template.Backend.Contract.Requests.Returns;

public class CreateReturnRequest : BaseRequest
{
    public Guid OrderId { get; set; }
    public Guid? OrderDetailId { get; set; }
    public Guid BuyerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<Guid> Items { get; set; } = new();
}
