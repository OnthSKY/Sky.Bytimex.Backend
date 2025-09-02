using System;

namespace Sky.Template.Backend.Contract.Responses.ProductResponses;

public class ProductStockDto
{
    public Guid ProductId { get; set; }
    public decimal StockQuantity { get; set; }
    public bool IsInStock => StockQuantity > 0;
}

