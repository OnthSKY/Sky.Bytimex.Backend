using Sky.Template.Backend.Contract.Requests;

namespace Sky.Template.Backend.Contract.Requests.Products;

public class CreateProductCategoryRequest : BaseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
} 