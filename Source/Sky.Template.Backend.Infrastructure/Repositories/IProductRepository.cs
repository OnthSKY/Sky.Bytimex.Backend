using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Configs.Products;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Core.Requests.Base;
using Microsoft.AspNetCore.Http;
using System.Data.Common;
using Sky.Template.Backend.Core.Context;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IProductRepository : IRepository<ProductEntity, Guid>
{
    Task<ProductEntity?> GetProductBySkuAsync(string sku);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeId = null);
    Task<(IEnumerable<ProductLocalizedJoinEntity>, int TotalCount)> GetLocalizedProductsAsync(ProductFilterRequest request, bool onlyActive = false);
    Task<Guid> AddProductImageAsync(Guid productId, string imageUrl, string? altText, int? sortOrder, bool isPrimary);
    Task UpsertProductAttributeAsync(Guid productId, string attributeCode, string attributeName, string valueText);
    Task<Guid> CreateVariantAsync(Guid productId, string sku, decimal? price, decimal? stockQuantity);
    Task SetVariantAttributeAsync(Guid variantId, string attributeCode, string valueText);
    Task<(IEnumerable<ProductVariantEntity>, int TotalCount)> GetProductVariantsAsync(Guid productId, GridRequest request);
    Task<int> GetActiveProductCountAsync(Guid vendorId, DbConnection connection, DbTransaction transaction);
    Task InsertAsync(ProductEntity product, DbConnection connection, DbTransaction transaction);
    Task UpdateAsync(ProductEntity product, DbConnection connection, DbTransaction transaction);
}

public class ProductRepository : Repository<ProductEntity, Guid>, IProductRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductRepository(IHttpContextAccessor httpContextAccessor) : base(new GridQueryConfig<ProductEntity>
    {
        BaseSql = "SELECT * FROM sys.products p WHERE p.is_deleted = FALSE",
        ColumnMappings = ProductGridFilterConfig.GetColumnMappings(),
        DefaultOrderBy = ProductGridFilterConfig.GetDefaultOrder(),
        LikeFilterKeys = ProductGridFilterConfig.GetLikeFilterKeys(),
        SearchColumns = ProductGridFilterConfig.GetSearchColumns()
    })
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ProductEntity?> GetProductBySkuAsync(string sku)
    {
        var query = "SELECT * FROM sys.products WHERE sku = @sku AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<ProductEntity>(query, new Dictionary<string, object> { { "@sku", sku } });
        return result.FirstOrDefault();
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeId = null)
    {
        var query = excludeId.HasValue
            ? "SELECT COUNT(*) FROM sys.products WHERE sku = @sku AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.products WHERE sku = @sku AND is_deleted = FALSE";

        var parameters = excludeId.HasValue
            ? new Dictionary<string, object> { { "@sku", sku }, { "@excludeId", excludeId.Value } }
            : new Dictionary<string, object> { { "@sku", sku } };

        var count = await DbManager.ReadAsync<DataCountEntity>(query, parameters);
        return count.FirstOrDefault()?.Count == 0;
    }

    public async Task<(IEnumerable<ProductLocalizedJoinEntity>, int TotalCount)> GetLocalizedProductsAsync(ProductFilterRequest request, bool onlyActive = false)
    {
        var baseSql = @"SELECT p.id,
                                p.vendor_id,
                                p.slug,
                                p.price,
                                p.status,
                                (SELECT image_url FROM sys.product_images i WHERE i.product_id = p.id AND i.is_primary = TRUE ORDER BY i.sort_order ASC LIMIT 1) AS primary_image_url,
                                COALESCE(pt_lang.name, pt_any.name) AS name,
                                COALESCE(pt_lang.description, pt_any.description) AS description,
                                p.category_id,
                                p.product_type,
                                p.unit,
                                p.barcode,
                                p.stock_quantity,
                                p.is_stock_tracked,
                                p.sku,
                                p.is_decimal_quantity_allowed,
                                p.is_deleted,
                                p.created_at,
                                p.created_by,
                                p.updated_at,
                                p.updated_by,
                                p.deleted_at,
                                p.deleted_by,
                                p.delete_reason
                         FROM sys.products p
                         LEFT JOIN LATERAL (
                            SELECT name, description FROM sys.product_translations
                            WHERE product_id = p.id AND language_code = @lang
                            LIMIT 1
                         ) pt_lang ON TRUE
                         LEFT JOIN LATERAL (
                            SELECT name, description FROM sys.product_translations
                            WHERE product_id = p.id
                            ORDER BY language_code
                            LIMIT 1
                         ) pt_any ON TRUE
                         WHERE p.is_deleted = FALSE" + (onlyActive ? " AND p.status = 'ACTIVE'" : string.Empty);

        var (sql, parameters) = GridQueryBuilder.Build(baseSql, request, ProductGridFilterConfig.GetColumnMappings(), ProductGridFilterConfig.GetDefaultOrder(), ProductGridFilterConfig.GetLikeFilterKeys(), ProductGridFilterConfig.GetSearchColumns());
        var languageCode = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString()?.Split(',').FirstOrDefault()?.ToLower() ?? "en";
        parameters["@lang"] = languageCode;
        var data = await DbManager.ReadAsync<ProductLocalizedJoinEntity>(sql, parameters);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters)).FirstOrDefault();
        return (data, count?.Count ?? 0);
    }

    public async Task<Guid> AddProductImageAsync(Guid productId, string imageUrl, string? altText, int? sortOrder, bool isPrimary)
    {
        if (isPrimary)
        {
            const string resetSql = "UPDATE sys.product_images SET is_primary = FALSE WHERE product_id = @pid";
            await DbManager.ExecuteNonQueryAsync(resetSql, new Dictionary<string, object> { { "@pid", productId } });
        }

        const string sql = @"INSERT INTO sys.product_images (product_id, image_url, alt_text, sort_order, is_primary)
                               VALUES (@productId, @imageUrl, @altText, @sortOrder, @isPrimary)
                               RETURNING id";

        var parameters = new Dictionary<string, object>
        {
            { "@productId", productId },
            { "@imageUrl", imageUrl },
            { "@altText", (object?)altText ?? DBNull.Value },
            { "@sortOrder", sortOrder ?? 0 },
            { "@isPrimary", isPrimary }
        };

        return await DbManager.ExecuteScalarAsync<Guid>(sql, parameters);
    }

    public async Task UpsertProductAttributeAsync(Guid productId, string attributeCode, string attributeName, string valueText)
    {
        const string attrSql = @"WITH ins AS (
                                    INSERT INTO sys.product_attributes (code, name)
                                    VALUES (@code, @name)
                                    ON CONFLICT (code) DO UPDATE SET name = EXCLUDED.name
                                    RETURNING id
                                   )
                                   SELECT id FROM ins UNION SELECT id FROM sys.product_attributes WHERE code = @code LIMIT 1";

        var attributeId = await DbManager.ExecuteScalarAsync<Guid>(attrSql, new Dictionary<string, object>
        {
            {"@code", attributeCode},
            {"@name", attributeName}
        });

        const string valueSql = @"INSERT INTO sys.product_attribute_values (product_id, attribute_id, value_text)
                                   VALUES (@pid, @aid, @val)
                                   ON CONFLICT DO NOTHING";

        await DbManager.ExecuteNonQueryAsync(valueSql, new Dictionary<string, object>
        {
            {"@pid", productId},
            {"@aid", attributeId},
            {"@val", valueText}
        });
    }

    public async Task<Guid> CreateVariantAsync(Guid productId, string sku, decimal? price, decimal? stockQuantity)
    {
        const string sql = @"INSERT INTO sys.product_variants (product_id, sku, price, stock_quantity)
                              VALUES (@pid, @sku, @price, @stock)
                              RETURNING id";

        var parameters = new Dictionary<string, object>
        {
            {"@pid", productId},
            {"@sku", sku},
            {"@price", price ?? (object)DBNull.Value},
            {"@stock", stockQuantity ?? (object)DBNull.Value}
        };

        return await DbManager.ExecuteScalarAsync<Guid>(sql, parameters);
    }

    public async Task SetVariantAttributeAsync(Guid variantId, string attributeCode, string valueText)
    {
        const string attrSql = @"WITH ins AS (
                                    INSERT INTO sys.product_attributes (code, name)
                                    VALUES (@code, @code)
                                    ON CONFLICT (code) DO NOTHING
                                    RETURNING id
                                   )
                                   SELECT id FROM ins UNION SELECT id FROM sys.product_attributes WHERE code = @code LIMIT 1";

        var attributeId = await DbManager.ExecuteScalarAsync<Guid>(attrSql, new Dictionary<string, object> {{"@code", attributeCode}});

        const string valueSql = @"INSERT INTO sys.product_variant_attribute_values (variant_id, attribute_id, value_text)
                                   VALUES (@vid, @aid, @val)
                                   ON CONFLICT DO NOTHING";

        await DbManager.ExecuteNonQueryAsync(valueSql, new Dictionary<string, object>
        {
            {"@vid", variantId},
            {"@aid", attributeId},
            {"@val", valueText}
        });
    }

    public async Task<(IEnumerable<ProductVariantEntity>, int TotalCount)> GetProductVariantsAsync(Guid productId, GridRequest request)
    {
        var baseSql = @"SELECT v.id,
                                v.product_id,
                                v.sku,
                                v.price,
                                v.stock_quantity,
                                v.is_active,
                                v.created_at
                         FROM sys.product_variants v
                         WHERE v.product_id = @productId";

        var (sql, parameters) = GridQueryBuilder.Build(baseSql, request, ProductVariantGridConfig.GetColumnMappings(), ProductVariantGridConfig.GetDefaultOrder(), ProductVariantGridConfig.GetLikeFilterKeys(), ProductVariantGridConfig.GetSearchColumns());
        parameters["@productId"] = productId;

        var data = await DbManager.ReadAsync<ProductVariantEntity>(sql, parameters);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters)).FirstOrDefault();
        return (data, count?.Count ?? 0);
    }

    public async Task<int> GetActiveProductCountAsync(Guid vendorId, DbConnection connection, DbTransaction transaction)
    {
        const string sql = "SELECT COUNT(*) AS count FROM sys.products WHERE vendor_id = @vendorId AND status IN ('ACTIVE','PUBLISHED') AND is_deleted = FALSE";
        var data = await DbManager.ReadAsync<DataCountEntity>(sql, new Dictionary<string, object> { { "@vendorId", vendorId } }, connection, transaction, GlobalSchema.Name);
        return data.FirstOrDefault()?.Count ?? 0;
    }

    public async Task InsertAsync(ProductEntity product, DbConnection connection, DbTransaction transaction)
    {
        const string sql = @"INSERT INTO sys.products (id, vendor_id, category_id, product_type, price, stock_quantity, is_stock_tracked, sku, status, created_at, created_by, is_deleted)
                             VALUES (@id, @vendor_id, @category_id, @product_type, @price, @stock_quantity, @is_stock_tracked, @sku, @status, @created_at, @created_by, FALSE)";
        var parameters = new Dictionary<string, object>
        {
            {"@id", product.Id},
            {"@vendor_id", product.VendorId},
            {"@category_id", product.CategoryId ?? (object)DBNull.Value},
            {"@product_type", product.ProductType},
            {"@price", product.Price},
            {"@stock_quantity", product.StockQuantity ?? (object)DBNull.Value},
            {"@is_stock_tracked", product.IsStockTracked},
            {"@sku", product.Sku ?? (object)DBNull.Value},
            {"@status", product.Status},
            {"@created_at", product.CreatedAt},
            {"@created_by", product.CreatedBy}
        };
        await DbManager.ExecuteTransactionNonQueryWithAsync(sql, parameters, connection, transaction, GlobalSchema.Name);
    }

    public async Task UpdateAsync(ProductEntity product, DbConnection connection, DbTransaction transaction)
    {
        const string sql = @"UPDATE sys.products
                             SET category_id = @category_id,
                                 product_type = @product_type,
                                 price = @price,
                                 stock_quantity = @stock_quantity,
                                 is_stock_tracked = @is_stock_tracked,
                                 sku = @sku,
                                 status = @status,
                                 updated_at = @updated_at,
                                 updated_by = @updated_by
                             WHERE id = @id";

        var parameters = new Dictionary<string, object>
        {
            {"@id", product.Id},
            {"@category_id", product.CategoryId ?? (object)DBNull.Value},
            {"@product_type", product.ProductType},
            {"@price", product.Price},
            {"@stock_quantity", product.StockQuantity ?? (object)DBNull.Value},
            {"@is_stock_tracked", product.IsStockTracked},
            {"@sku", product.Sku ?? (object)DBNull.Value},
            {"@status", product.Status},
            {"@updated_at", product.UpdatedAt},
            {"@updated_by", product.UpdatedBy}
        };

        await DbManager.ExecuteTransactionNonQueryWithAsync(sql, parameters, connection, transaction, GlobalSchema.Name);
    }
}