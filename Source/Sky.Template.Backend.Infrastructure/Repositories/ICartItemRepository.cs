using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface ICartItemRepository : IRepository<CartItemEntity, Guid>
{
    Task<IEnumerable<CartItemLocalizedJoinEntity>> GetByCartIdAsync(Guid cartId, string language);
    Task<CartItemLocalizedJoinEntity?> GetByIdWithProductAsync(Guid id, string language);
    Task<CartItemEntity?> GetByCartAndProductAsync(Guid cartId, Guid productId);
}

public class CartItemRepository : Repository<CartItemEntity, Guid>, ICartItemRepository
{
    public CartItemRepository() : base() { }

    public async Task<IEnumerable<CartItemLocalizedJoinEntity>> GetByCartIdAsync(Guid cartId, string language)
    {
        const string sql = @"SELECT ci.id, ci.cart_id, ci.product_id, ci.quantity, ci.unit_price, ci.currency, ci.status,
                                   ci.created_at, ci.created_by, ci.updated_at, ci.updated_by,
                                   ci.is_deleted, ci.deleted_at, ci.deleted_by, ci.delete_reason,
                                   COALESCE(pt.name, '') AS product_name
                            FROM sys.cart_items ci
                            LEFT JOIN sys.product_translations pt ON ci.product_id = pt.product_id AND pt.language_code = @lang
                            WHERE ci.cart_id = @cartId AND ci.is_deleted = FALSE";
        var parameters = new Dictionary<string, object>
        {
            {"@cartId", cartId},
            {"@lang", language}
        };
        return await DbManager.ReadAsync<CartItemLocalizedJoinEntity>(sql, parameters, GlobalSchema.Name);
    }

    public async Task<CartItemLocalizedJoinEntity?> GetByIdWithProductAsync(Guid id, string language)
    {
        const string sql = @"SELECT ci.id, ci.cart_id, ci.product_id, ci.quantity, ci.unit_price, ci.currency, ci.status,
                                   ci.created_at, ci.created_by, ci.updated_at, ci.updated_by,
                                   ci.is_deleted, ci.deleted_at, ci.deleted_by, ci.delete_reason,
                                   COALESCE(pt.name, '') AS product_name
                            FROM sys.cart_items ci
                            LEFT JOIN sys.product_translations pt ON ci.product_id = pt.product_id AND pt.language_code = @lang
                            WHERE ci.id = @id AND ci.is_deleted = FALSE";
        var parameters = new Dictionary<string, object>
        {
            {"@id", id},
            {"@lang", language}
        };
        var result = await DbManager.ReadAsync<CartItemLocalizedJoinEntity>(sql, parameters, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<CartItemEntity?> GetByCartAndProductAsync(Guid cartId, Guid productId)
    {
        const string sql = "SELECT * FROM sys.cart_items WHERE cart_id = @cartId AND product_id = @productId AND is_deleted = FALSE";
        var parameters = new Dictionary<string, object>
        {
            {"@cartId", cartId},
            {"@productId", productId}
        };
        var result = await DbManager.ReadAsync<CartItemEntity>(sql, parameters, GlobalSchema.Name);
        return result.FirstOrDefault();
    }
}
