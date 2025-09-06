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
using Sky.Template.Backend.Infrastructure.Queries;

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

    public async Task<(IEnumerable<ProductLocalizedJoinEntity>, int TotalCount)> GetLocalizedProductsAsync(
    ProductFilterRequest request, bool onlyActive = false)
    {
        // 0) Dil paramı (Accept-Language -> "tr", "en" ... )
        var languageCode = (_httpContextAccessor.HttpContext?
            .Request.Headers["Accept-Language"].ToString() ?? "en")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?.Trim().ToLower() ?? "en";
        if (languageCode.Length > 2) languageCode = languageCode[..2];

        // 1) Taban sorgu
        var baseSql = ProductQueries.GetLocalizedProductsAsync + (onlyActive ? " AND p.status = 'ACTIVE'" : "");

        // 2) Filtrelerden kategori/subcategory oku (Guid/CSV destekli)
        request.Filters ??= new Dictionary<string, string>();
        request.Filters.TryGetValue("categoryId", out var catRaw);
        request.Filters.TryGetValue("subcategoryId", out var subRaw);

        var catIds = ParseGuids(catRaw);   // List<Guid>
        var subIds = ParseGuids(subRaw);   // List<Guid>

        var hasCategory = !string.IsNullOrWhiteSpace(catRaw);
        var hasSubCat = !string.IsNullOrWhiteSpace(subRaw);

        // 3) CTE ve WHERE (gerekiyorsa) + paramlar
        var cteParams = new Dictionary<string, object>();
        if (hasCategory || hasSubCat)
        {
            baseSql = $@"
WITH RECURSIVE roots AS (
    SELECT UNNEST(COALESCE(@categoryIds,    ARRAY[]::uuid[])) AS id
    UNION
    SELECT UNNEST(COALESCE(@subcategoryIds, ARRAY[]::uuid[]))
),
subcats AS (
    SELECT id
    FROM sys.product_categories
    WHERE id IN (SELECT id FROM roots)
  UNION ALL
    SELECT c.id
    FROM sys.product_categories c
    JOIN subcats s ON c.parent_category_id = s.id
)
{baseSql}
  AND p.category_id IN (SELECT id FROM subcats)
";
            // Parametreleri her koşulda bağla (boş dizi bile olsa)
            cteParams["@categoryIds"] = catIds.Count > 0 ? catIds.ToArray() : Array.Empty<Guid>();
            cteParams["@subcategoryIds"] = subIds.Count > 0 ? subIds.ToArray() : Array.Empty<Guid>();
        }

        // 4) GridQueryBuilder’a giderken cat/sub filtre anahtarlarını temizle
        var sanitized = Utils.CloneWithoutKeys(request, new[] { "categoryId", "subcategoryId" });

        // 5) Builder
        var mappings = ProductGridFilterConfig.GetColumnMappings();
        var (sql, builtParams) = GridQueryBuilder.Build(
            baseSql,
            sanitized,
            mappings,
            ProductGridFilterConfig.GetDefaultOrder(),
            ProductGridFilterConfig.GetLikeFilterKeys(),
            ProductGridFilterConfig.GetSearchColumns(),
            DbManager.Dialect
        );

        // 6) Dil ve CTE paramlarını birleştir
        builtParams["@lang"] = languageCode;
        foreach (var kv in cteParams) builtParams[kv.Key] = kv.Value;

        // 7) Çalıştır (şema gerekiyorsa üçüncü argümanla ver)
        var data = await DbManager.ReadAsync<ProductLocalizedJoinEntity>(sql, builtParams /*, GlobalSchema.Name*/);

        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, builtParams /*, GlobalSchema.Name*/))
                    .FirstOrDefault()?.Count ?? 0;

        return (data, count);

        // --- local helpers ---
        static List<Guid> ParseGuids(string? raw)
        {
            var list = new List<Guid>();
            if (string.IsNullOrWhiteSpace(raw)) return list;
            foreach (var s in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (Guid.TryParse(s, out var g)) list.Add(g);
            return list;
        }
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

        var attributeId = await DbManager.ExecuteScalarAsync<Guid>(attrSql, new Dictionary<string, object> { { "@code", attributeCode } });

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

        var (sql, parameters) = GridQueryBuilder.Build(baseSql, request, ProductVariantGridConfig.GetColumnMappings(), ProductVariantGridConfig.GetDefaultOrder(), ProductVariantGridConfig.GetLikeFilterKeys(), ProductVariantGridConfig.GetSearchColumns(), DbManager.Dialect);
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