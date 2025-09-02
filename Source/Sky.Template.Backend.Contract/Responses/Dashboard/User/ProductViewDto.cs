using System;

namespace Sky.Template.Backend.Contract.Responses.Dashboard.User;

public class ProductViewDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
}
