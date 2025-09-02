using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Contract.Responses.CartResponses;

public class CartListPaginatedResponse
{
    public PaginatedData<CartResponse> Carts { get; set; } = new();
}
