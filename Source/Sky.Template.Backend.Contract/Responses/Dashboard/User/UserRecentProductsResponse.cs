using System.Collections.Generic;

namespace Sky.Template.Backend.Contract.Responses.Dashboard.User;

public class UserRecentProductsResponse
{
    public List<ProductViewDto> Products { get; set; } = new();
}
