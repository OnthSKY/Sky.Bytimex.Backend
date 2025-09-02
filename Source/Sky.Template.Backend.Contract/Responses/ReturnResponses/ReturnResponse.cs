using Sky.Template.Backend.Core.BaseResponse;
using System.Collections.Generic;
using System.Linq;

namespace Sky.Template.Backend.Contract.Responses.ReturnResponses;

public class ReturnResponse : BaseServiceResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? OrderDetailId { get; set; }
    public Guid BuyerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public IEnumerable<Guid> Items { get; set; } = Enumerable.Empty<Guid>();
}
