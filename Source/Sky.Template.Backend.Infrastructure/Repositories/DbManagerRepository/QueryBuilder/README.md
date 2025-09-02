# QueryBuilder (Q)

Fluent SQL builder used with `DbManager` and dialect abstraction.

```csharp
var columnMappings = new Dictionary<string,string>
{
    ["name"] = "p.name",
    ["sku"]  = "p.sku",
    ["created_at"] = "p.created_at"
};
var likeKeys = new HashSet<string> { "name", "sku" };

var qb = Q.From<ProductEntity>("p")
    .Select("p.id", "p.name")
    .WithSearch(req.SearchValue, new[]{"p.name", "p.sku"})
    .WithFilters(req.Filters, columnMappings, likeKeys)
    .OrderByMapped(req.OrderColumn, req.OrderDirection, columnMappings, "p.created_at DESC")
    .Paginate(req.Page, req.PageSize);

var (sql, @params) = qb.Build();
var countSql = qb.ToCountSql();
```

* Search and filters are parameterized to avoid SQL injection.
* Ordering uses whitelisted mappings with safe fallback.
* Pagination uses dialect-specific `Paginate` and binds `@Offset` / `@PageSize`.
* `ToCountSql()` clones the query, strips top-level `ORDER BY` and wraps with the dialect's count helper.
