using System;

namespace Sky.Template.Backend.Contract.Requests.Products;

public class SetVariantAttributeRequest
{
    public Guid VariantId { get; set; }
    public string AttributeCode { get; set; } = string.Empty;
    public string ValueText { get; set; } = string.Empty;
}

