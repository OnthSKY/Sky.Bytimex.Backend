namespace Sky.Template.Backend.Infrastructure.Queries;

public static class ProductQueries
{
    public const string GetLocalizedProductsAsync = @"
        SELECT p.id,
               p.vendor_id,
               p.slug,
               p.price,
               p.status,
               (SELECT image_url FROM sys.product_images i 
                 WHERE i.product_id = p.id AND i.is_primary = TRUE 
                 ORDER BY i.sort_order ASC LIMIT 1) AS primary_image_url,
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
            SELECT name, description
            FROM sys.product_translations
            WHERE product_id = p.id AND language_code = @lang
            LIMIT 1
        ) pt_lang ON TRUE
        LEFT JOIN LATERAL (
            SELECT name, description
            FROM sys.product_translations
            WHERE product_id = p.id
            ORDER BY language_code
            LIMIT 1
        ) pt_any ON TRUE
        WHERE p.is_deleted = FALSE";
}