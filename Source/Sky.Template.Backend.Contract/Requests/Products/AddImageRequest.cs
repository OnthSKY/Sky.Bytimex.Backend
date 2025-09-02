using System;

namespace Sky.Template.Backend.Contract.Requests.Products;

public class AddImageRequest
{
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int? SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

