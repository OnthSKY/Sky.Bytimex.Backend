using Sky.Template.Backend.Core.Requests.Base;

namespace Sky.Template.Backend.Contract.Requests.Products;

public class ProductCategoryFilterRequest : GridRequest
{
    public string? Name { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? Description { get; set; }
}
