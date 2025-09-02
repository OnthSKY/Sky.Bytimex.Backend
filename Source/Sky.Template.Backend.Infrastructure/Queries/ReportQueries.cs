namespace Sky.Template.Backend.Infrastructure.Queries;

internal static class ReportQueries
{
    internal const string OrdersByVendor = @"
        SELECT
            s.vendor_id,
            v.name AS vendor_name,
            SUM(s.total_amount) AS total_amount,
            COUNT(*) AS order_count
        FROM sys.orders s
        LEFT JOIN sys.vendors v ON v.id = s.vendor_id
        WHERE s.is_deleted = FALSE
          AND (@startDate IS NULL OR s.order_date >= @startDate)
          AND (@endDate IS NULL OR s.order_date <= @endDate)
        GROUP BY s.vendor_id, vendor_name
        ORDER BY total_amount DESC;";

    internal const string OrdersByCategory = @"
        SELECT
            p.category_id,
            pc.name AS category_name,
            SUM(sd.quantity * sd.unit_price - sd.discount) AS total_amount,
            SUM(sd.quantity) AS order_count
        FROM sys.orders_details sd
        INNER JOIN sys.orders s ON s.id = sd.order_id AND s.is_deleted = FALSE
        INNER JOIN sys.products p ON p.id = sd.product_id
        LEFT JOIN sys.product_categories pc ON pc.id = p.category_id
        WHERE (@startDate IS NULL OR s.order_date >= @startDate)
          AND (@endDate IS NULL OR s.order_date <= @endDate)
        GROUP BY p.category_id, pc.name
        ORDER BY total_amount DESC;";

    internal const string OrdersByProduct = @"
        SELECT
            p.id AS product_id,
            p.name AS product_name,
            SUM(sd.quantity * sd.unit_price - sd.discount) AS total_amount,
            SUM(sd.quantity) AS order_count
        FROM sys.orders_details sd
        INNER JOIN sys.orders s ON s.id = sd.order_id AND s.is_deleted = FALSE
        INNER JOIN sys.products p ON p.id = sd.product_id
        WHERE (@startDate IS NULL OR s.order_date >= @startDate)
          AND (@endDate IS NULL OR s.order_date <= @endDate)
        GROUP BY p.id, p.name
        ORDER BY total_amount DESC;";

    internal const string OrdersByPeriodTemplate = @"
        SELECT
            to_char(date_trunc('{period}', s.order_date), 'YYYY-MM-DD') AS period,
            SUM(s.total_amount) AS total_amount,
            COUNT(*) AS order_count
        FROM sys.orders s
        WHERE s.is_deleted = FALSE
          AND (@startDate IS NULL OR s.order_date >= @startDate)
          AND (@endDate IS NULL OR s.order_date <= @endDate)
        GROUP BY date_trunc('{period}', s.order_date)
        ORDER BY period;";

    internal const string VendorCount = @"
        SELECT COUNT(*) AS count
        FROM sys.vendors v
        WHERE v.is_deleted = FALSE;";

    internal const string VendorOrdersSummary = @"
        SELECT
            vendor_id,
            SUM(total_amount) AS total_amount,
            COUNT(*) AS order_count
        FROM vw_vendor_orders_summary
        WHERE (@vendor_id IS NULL OR vendor_id = @vendor_id)
          AND (@startDate IS NULL OR order_date >= @startDate)
          AND (@endDate IS NULL OR order_date <= @endDate)
          AND (@currency IS NULL OR currency = @currency)
        GROUP BY vendor_id";
}
