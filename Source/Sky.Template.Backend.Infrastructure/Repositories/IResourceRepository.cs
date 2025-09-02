using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Queries;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using System.Text;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IResourceRepository
{
    Task<IEnumerable<ResourceEntity>> GetAllAsync();
    Task<ResourceEntity?> GetByCodeAsync(string code);
    Task<IEnumerable<ResourceEntity>> GetByUserAsync(Guid userId);
    Task<ResourceEntity> CreateAsync(ResourceEntity entity);
    Task<ResourceEntity> UpdateAsync(ResourceEntity entity);
    Task<bool> DeleteAsync(string code);
}

public class ResourceRepository : IResourceRepository
{
    public async Task<IEnumerable<ResourceEntity>> GetAllAsync()
    {
        return await DbManager.ReadAsync<ResourceEntity>(ResourceQueries.GetActiveResources, new Dictionary<string, object>(), GlobalSchema.Name);
    }

    public async Task<ResourceEntity?> GetByCodeAsync(string code)
    {
        var sql = "SELECT * FROM sys.resources WHERE code = @code";
        var result = await DbManager.ReadAsync<ResourceEntity>(sql, new Dictionary<string, object> { { "@code", code } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<ResourceEntity>> GetByUserAsync(Guid userId)
    {
        var sql = @"SELECT DISTINCT r.code, r.name, r.description
                     FROM sys.resources r
                     INNER JOIN sys.permissions p ON p.resource_id = r.id
                     INNER JOIN sys.role_permissions rp ON rp.permission_id = p.id
                     INNER JOIN sys.user_roles ur ON ur.role_id = rp.role_id
                     WHERE ur.user_id = @userId 
                     ORDER BY r.code";
        var parameters = new Dictionary<string, object>
        {
            {"@userId", userId}
        };
        return await DbManager.ReadAsync<ResourceEntity>(sql, parameters, GlobalSchema.Name);
    }

    public async Task<ResourceEntity> CreateAsync(ResourceEntity entity)
    {
        var sql = @"INSERT INTO sys.resources (code, name, description, created_at, created_by)
                     VALUES (@code, @name, @description, @createdAt, @createdBy)
                     RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@code", entity.Code},
            {"@name", entity.Name},
            {"@description", entity.Description},
            {"@createdAt", entity.CreatedAt},
            {"@createdBy", entity.CreatedBy}
        };
        var result = await DbManager.ReadAsync<ResourceEntity>(sql, parameters, GlobalSchema.Name);
        return result.First();
    }

    public async Task<ResourceEntity> UpdateAsync(ResourceEntity entity)
    {
        var sql = @"UPDATE sys.resources SET name = @name, description = @description, updated_at = @updatedAt, updated_by = @updatedBy
                     WHERE code = @code RETURNING *";
        var parameters = new Dictionary<string, object>
        {
            {"@code", entity.Code},
            {"@name", entity.Name},
            {"@description", entity.Description},
            {"@updatedAt", entity.UpdatedAt},
            {"@updatedBy", entity.UpdatedBy}
        };
        var result = await DbManager.ReadAsync<ResourceEntity>(sql, parameters, GlobalSchema.Name);
        return result.First();
    }

    public async Task<bool> DeleteAsync(string code)
    {
        var sql = "DELETE FROM sys.resources WHERE code = @code";
        return await DbManager.ExecuteNonQueryAsync(sql, new Dictionary<string, object> { { "@code", code } }, GlobalSchema.Name);
    }
}
