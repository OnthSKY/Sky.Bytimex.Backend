

using Sky.Template.Backend.Contract.Requests.Roles;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Infrastructure.Entities.UserRole;
using Sky.Template.Backend.Infrastructure.Queries;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Configs.Roles;
using Sky.Template.Backend.Infrastructure.Repositories.Base;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IRoleRepository : IRepository<RoleEntity, int>
{
    #region Read-Interfaces
    Task<IEnumerable<RoleEntity>> GetAllRolesWithUserCountAsync();
    Task<RoleEntity> GetRoleByIdAsync(int id);
    Task<(IEnumerable<BaseUserEntity>, int TotalCount)> GetFilteredPaginatedUsersByRoleId(GetUsersByRoleRequest request);
    #endregion

    #region Transaction-Interfaces
    Task<bool> UpdateUserRoleAsync(UpdateUserRoleRequest request, Guid updaterUserId);
    Task<bool> AddPermissionToRoleAsync(int roleId, int permissionId, Guid creatorId);
    #endregion
}

public class RoleRepository : Repository<RoleEntity, int>, IRoleRepository
{
    public async Task<IEnumerable<RoleEntity>> GetAllRolesWithUserCountAsync()
    {
        return await DbManager.ReadAsync<RoleEntity>(RoleQueries.GetAllRolesWithUserCount, null, GlobalSchema.Name);
    }

    public async Task<RoleEntity> GetRoleByIdAsync(int id)
    {
        return (await DbManager.ReadAsync<RoleEntity>(RoleQueries.GetRoleById, new Dictionary<string, object>
        {
            {"@id", id}
        }, GlobalSchema.Name)).FirstOrDefault();
    }

    public async Task<(IEnumerable<BaseUserEntity>, int TotalCount)> GetFilteredPaginatedUsersByRoleId(GetUsersByRoleRequest request)
    {
        var columnMappingsForFilters = new Dictionary<string, string>()
        {
            {"userId", "u.id"},
            {"firstName", "u.first_name"},
            {"lastName", "u.last_name"},
            {"email", "u.email"},
        };

        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: RoleQueries.GetFilteredPaginatedUsersByRoleId,
            request: request,
            columnMappings: columnMappingsForFilters,
            defaultOrderBy: "u.first_name ASC",
            likeFilterKeys: new HashSet<string> { "firstName", "lastName", "email" },
            searchColumns: new List<string> { "u.first_name", "u.last_name", "u.email" }
        );

        var data = await DbManager.ReadAsync<BaseUserEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();
        return (data, count?.Count ?? 0);
    }

    public async Task<bool> UpdateUserRoleAsync(UpdateUserRoleRequest request, Guid updaterUserId)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@userId", request.UserId},
            {"@roleId", request.RoleId},
            {"@updatedAt", DateTime.UtcNow},
            {"@updatedBy", updaterUserId}
        };

        var affectedRows = await DbManager.ExecuteNonQueryAsync(RoleQueries.UpdateUserRole, parameters, GlobalSchema.Name);
        return affectedRows;
    }

    public async Task<bool> AddPermissionToRoleAsync(int roleId, int permissionId, Guid creatorId)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@roleId", roleId},
            {"@permissionId", permissionId},
            {"@createdAt", DateTime.UtcNow},
            {"@createdBy", creatorId}
        };

        var affected = await DbManager.ExecuteNonQueryAsync(RoleQueries.AddPermissionToRole, parameters, GlobalSchema.Name);
        return affected;
    }

    public override async Task<(IEnumerable<RoleEntity>, int TotalCount)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var baseSql = "SELECT * FROM sys.roles r WHERE r.is_deleted = FALSE";
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: RoleGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: RoleGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: RoleGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: RoleGridFilterConfig.GetSearchColumns()
        );

        var data = await DbManager.ReadAsync<RoleEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();
        
        return (data, count?.Count ?? 0);
    }
}
