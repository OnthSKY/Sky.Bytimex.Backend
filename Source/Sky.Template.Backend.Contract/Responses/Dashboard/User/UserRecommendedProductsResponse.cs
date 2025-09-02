using System.Collections.Generic;

namespace Sky.Template.Backend.Contract.Responses.Dashboard.User;

public class UserRecommendedProductsResponse
{
    public List<ProductViewDto> Products { get; set; } = new();
}
