using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Configs.Products;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IProductCategoryRepository : IRepository<ProductCategoryEntity, Guid>
{
    Task<ProductCategoryEntity?> GetCategoryByNameAsync(string name);
    Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null);
}

public class ProductCategoryRepository : Repository<ProductCategoryEntity, Guid>, IProductCategoryRepository
{
    public ProductCategoryRepository() : base(new GridQueryConfig<ProductCategoryEntity>
    {
        BaseSql = "SELECT * FROM sys.product_categories pc WHERE pc.is_deleted = FALSE",
        ColumnMappings = ProductCategoryGridFilterConfig.GetColumnMappings(),
        DefaultOrderBy = ProductCategoryGridFilterConfig.GetDefaultOrder(),
        LikeFilterKeys = ProductCategoryGridFilterConfig.GetLikeFilterKeys(),
        SearchColumns = ProductCategoryGridFilterConfig.GetSearchColumns()
    }) { }

    public async Task<ProductCategoryEntity?> GetCategoryByNameAsync(string name)
    {
        var query = "SELECT * FROM sys.product_categories WHERE name = @name AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<ProductCategoryEntity>(query, new Dictionary<string, object> { { "@name", name } });
        return result.FirstOrDefault();
    }

    public async Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = excludeId.HasValue
            ? "SELECT COUNT(*) FROM sys.product_categories WHERE name = @name AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.product_categories WHERE name = @name AND is_deleted = FALSE";

        var parameters = excludeId.HasValue
            ? new Dictionary<string, object> { { "@name", name }, { "@excludeId", excludeId.Value } }
            : new Dictionary<string, object> { { "@name", name } };

        var count = await DbManager.ReadAsync<DataCountEntity>(query, parameters);
        return count.FirstOrDefault()?.Count == 0;
    }
} 