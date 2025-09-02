using FluentValidation;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Contract.Responses.StockMovementResponses;
using Sky.Template.Backend.Infrastructure.Entities.Stock;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Core.Exceptions;

namespace Sky.Template.Backend.Application.Services;

public abstract class StockMovementServiceBase
{
    protected readonly IStockMovementRepository _movementRepository;
    protected readonly IProductRepository _productRepository;
    protected readonly IHttpContextAccessor _httpContextAccessor;

    protected StockMovementServiceBase(IStockMovementRepository movementRepository,
        IProductRepository productRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected Guid GetUserId() => _httpContextAccessor.HttpContext.GetUserId();

    protected static decimal CalculateDelta(string type, decimal quantity, decimal currentStock) => type switch
    {
        "IN" => quantity,
        "RETURN" => quantity,
        "OUT" => -quantity,
        "CORRECTION" => quantity - currentStock,
        _ => throw new ValidationException("InvalidMovementType")
    };

    protected static StockMovementResponse MapToResponse(StockMovementEntity entity) => new()
    {
        Id = entity.Id,
        ProductId = entity.ProductId,
        SupplierId = entity.SupplierId,
        MovementType = entity.MovementType,
        Quantity = entity.Quantity,
        MovementDate = entity.MovementDate,
        Description = entity.Description,
        RelatedOrderId = entity.RelatedOrderId,
        Status = entity.Status,
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy
    };
}
