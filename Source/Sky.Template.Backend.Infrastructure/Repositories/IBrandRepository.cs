using Sky.Template.Backend.Contract.Requests.BrandRequests;
using Sky.Template.Backend.Infrastructure.Entities.Brand;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Sky.Template.Backend.Infrastructure.Configs.Brands;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IBrandRepository
{
    Task<bool> CodeExistsAsync(string slug, Guid? excludeId = null);
    Task<(IEnumerable<BrandListItemEntity> Items, int TotalCount)> GetPagedAsync(BrandFilterRequest request, string? preferredLang, bool onlyActive);
    Task<BrandEntity?> GetWithTranslationsAsync(Guid id);
    Task<Guid> CreateAsync(BrandEntity brand);
    Task UpsertTranslationsAsync(Guid brandId, IEnumerable<BrandTranslationEntity> translations);
    Task UpdateAsync(BrandEntity brand);
    Task SoftDeleteAsync(Guid id, Guid userId);
    Task HardDeleteAsync(Guid id);
}

public class BrandRepository : IBrandRepository
{
    public async Task<bool> CodeExistsAsync(string slug, Guid? excludeId = null)
    {
        var sql = excludeId.HasValue
            ? "SELECT COUNT(1) FROM sys.brands WHERE slug = @slug AND id != @id AND deleted_at IS NULL"
            : "SELECT COUNT(1) FROM sys.brands WHERE slug = @slug AND deleted_at IS NULL";
        var parameters = new Dictionary<string, object>{{"@slug", slug}};
        if (excludeId.HasValue) parameters["@id"] = excludeId.Value;
        var count = await DbManager.ReadAsync<DataCountEntity>(sql, parameters, GlobalSchema.Name);
        return count.FirstOrDefault()?.Count > 0;
    }

    public async Task<(IEnumerable<BrandListItemEntity> Items, int TotalCount)> GetPagedAsync(BrandFilterRequest request, string? preferredLang, bool onlyActive)
    {
        var baseSql = "SELECT b.id, b.slug, b.logo_url, b.status, bt.name FROM sys.brands b LEFT JOIN sys.brand_translations bt ON bt.brand_id = b.id AND bt.language_code = @lang WHERE b.deleted_at IS NULL";
        if (onlyActive)
            baseSql += " AND b.status = 'ACTIVE'";
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: BrandGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: BrandGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: BrandGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: BrandGridFilterConfig.GetSearchColumns()
        );
        parameters["@lang"] = preferredLang ?? "en";
        var data = await DbManager.ReadAsync<BrandListItemEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name);
        return (data, count.FirstOrDefault()?.Count ?? 0);
    }

    public async Task<BrandEntity?> GetWithTranslationsAsync(Guid id)
    {
        const string brandSql = "SELECT * FROM sys.brands WHERE id = @id AND deleted_at IS NULL";
        var brand = (await DbManager.ReadAsync<BrandEntity>(brandSql, new Dictionary<string, object>{{"@id", id}}, GlobalSchema.Name)).FirstOrDefault();
        if (brand == null) return null;
        const string trSql = "SELECT * FROM sys.brand_translations WHERE brand_id = @id";
        var translations = await DbManager.ReadAsync<BrandTranslationEntity>(trSql, new Dictionary<string, object>{{"@id", id}}, GlobalSchema.Name);
        brand.Translations = translations.ToList();
        return brand;
    }

    public async Task<Guid> CreateAsync(BrandEntity brand)
    {
        const string sql = "INSERT INTO sys.brands (id, slug, logo_url, status, created_at, created_by) VALUES (@id,@slug,@logo_url,@status,@created_at,@created_by)";
        var parameters = new Dictionary<string, object>
        {
            {"@id", brand.Id},
            {"@slug", brand.Slug},
            {"@logo_url", brand.LogoUrl ?? (object)DBNull.Value},
            {"@status", brand.Status},
            {"@created_at", brand.CreatedAt},
            {"@created_by", brand.CreatedBy ?? (object)DBNull.Value}
        };
        await DbManager.ExecuteNonQueryAsync(sql, parameters, GlobalSchema.Name);
        return brand.Id;
    }

    public async Task UpsertTranslationsAsync(Guid brandId, IEnumerable<BrandTranslationEntity> translations)
    {
        const string sql = @"INSERT INTO sys.brand_translations (brand_id, language_code, name, description)
                             VALUES (@brand_id,@lang,@name,@desc)
                             ON CONFLICT (brand_id, language_code)
                             DO UPDATE SET name = EXCLUDED.name, description = EXCLUDED.description";
        foreach (var tr in translations)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@brand_id", brandId},
                {"@lang", tr.LanguageCode},
                {"@name", tr.Name},
                {"@desc", tr.Description ?? (object)DBNull.Value}
            };
            await DbManager.ExecuteNonQueryAsync(sql, parameters, GlobalSchema.Name);
        }
    }

    public async Task UpdateAsync(BrandEntity brand)
    {
        const string sql = "UPDATE sys.brands SET slug=@slug, logo_url=@logo_url, status=@status, updated_at=@updated_at, updated_by=@updated_by WHERE id=@id";
        var parameters = new Dictionary<string, object>
        {
            {"@id", brand.Id},
            {"@slug", brand.Slug},
            {"@logo_url", brand.LogoUrl ?? (object)DBNull.Value},
            {"@status", brand.Status},
            {"@updated_at", brand.UpdatedAt ?? DateTime.UtcNow},
            {"@updated_by", brand.UpdatedBy ?? (object)DBNull.Value}
        };
        await DbManager.ExecuteNonQueryAsync(sql, parameters, GlobalSchema.Name);
    }

    public async Task SoftDeleteAsync(Guid id, Guid userId)
    {
        const string sql = "UPDATE sys.brands SET deleted_at = now(), deleted_by=@user WHERE id=@id";
        await DbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>{{"@id", id},{"@user", userId}}, GlobalSchema.Name);
    }

    public async Task HardDeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM sys.brands WHERE id=@id";
        await DbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object>{{"@id", id}}, GlobalSchema.Name);
    }
}
