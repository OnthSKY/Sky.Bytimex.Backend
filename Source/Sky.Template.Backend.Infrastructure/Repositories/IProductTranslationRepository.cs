using System.Data.Common;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IProductTranslationRepository
{
    Task<ProductTranslationEntity?> GetAsync(Guid productId, string languageCode);
    Task<ProductTranslationEntity> UpsertAsync(ProductTranslationEntity entity, DbConnection? connection = null, DbTransaction? transaction = null);
}

public class ProductTranslationRepository : IProductTranslationRepository
{
    public async Task<ProductTranslationEntity?> GetAsync(Guid productId, string languageCode)
    {
        const string sql = "SELECT * FROM sys.product_translations WHERE product_id = @productId AND language_code = @lang";
        var parameters = new Dictionary<string, object>
        {
            {"@productId", productId},
            {"@lang", languageCode}
        };
        var result = await DbManager.ReadAsync<ProductTranslationEntity>(sql, parameters, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<ProductTranslationEntity> UpsertAsync(ProductTranslationEntity entity, DbConnection? connection = null, DbTransaction? transaction = null)
    {
        const string sql = @"INSERT INTO sys.product_translations (product_id, language_code, name, description)
                             VALUES (@productId, @lang, @name, @description)
                             ON CONFLICT (product_id, language_code)
                             DO UPDATE SET name = EXCLUDED.name, description = EXCLUDED.description
                             RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@productId", entity.ProductId},
            {"@lang", entity.LanguageCode},
            {"@name", entity.Name},
            {"@description", entity.Description}
        };
        var data = connection != null && transaction != null
            ? await DbManager.ReadAsync<ProductTranslationEntity>(sql, parameters, connection, transaction, GlobalSchema.Name)
            : await DbManager.ReadAsync<ProductTranslationEntity>(sql, parameters, GlobalSchema.Name);
        return data.First();
    }
}
