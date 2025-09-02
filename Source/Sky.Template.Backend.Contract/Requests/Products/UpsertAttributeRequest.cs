using System;

namespace Sky.Template.Backend.Contract.Requests.Products;

public class UpsertAttributeRequest
{
    public Guid ProductId { get; set; }
    public string AttributeCode { get; set; } = string.Empty;
    public string AttributeName { get; set; } = string.Empty;
    public string ValueText { get; set; } = string.Empty;
}

