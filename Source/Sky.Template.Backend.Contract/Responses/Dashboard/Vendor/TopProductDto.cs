using System;

namespace Sky.Template.Backend.Contract.Responses.Dashboard.Vendor;

public class TopProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}
