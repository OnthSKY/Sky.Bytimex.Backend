using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Configs.Users;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Infrastructure.Queries;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;


namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IUserRepository : IRepository<UserEntity, Guid>
{
    #region Read-Interfaces
    Task<UserWithRoleEntity?> GetUserWithRoleByIdAsync(Guid userId);
    Task<UserWithRoleEntity?> GetUserWithRoleByEmailAsync(string email);
    Task<IEnumerable<DelegationEntity>> GetActiveDelegationsGivenByUserIdAsync(Guid userId);
    #region Paginated
    Task<(IEnumerable<UserWithRoleEntity>, int TotalCount)> GetAllUsersWithFilterAsync(UsersFilterRequest requset);
    #endregion

    #endregion

    #region Transaction-Interfaces
    Task<bool> UpdateUserImageFromAzureLoginAsync(string imagePath, string userId, string schemaName);
    Task<UserEntity?> UpdateUserAsync(UserEntity user);
    Task<bool> SoftDeleteUserAsync(Guid id, string reason);
    #endregion
}

public class UserRepository : Repository<UserEntity, Guid>, IUserRepository
{
    private string BuildUserWithRoleSql(string baseSql,string? extraWhereClause = null)
    {
        if (!string.IsNullOrWhiteSpace(extraWhereClause))
            return baseSql.Replace("/**extra_where**/", $" AND {extraWhereClause}");

        return baseSql.Replace("/**extra_where**/", string.Empty);
    }

    public async Task<UserWithRoleEntity?> GetUserWithRoleByIdAsync(Guid userId)
    {
        string baseSql = BuildUserWithRoleSql(UserQueries.GetActiveUsersWithRole, "u.id = @user_id");
        return (await DbManager.ReadAsync<UserWithRoleEntity>(baseSql, new Dictionary<string, object>
        {
            {"@user_id", userId}
        }, GlobalSchema.Name)).FirstOrDefault();
    }

    public async Task<UserWithRoleEntity?> GetUserWithRoleByEmailAsync(string email)
    {
        string baseSql = BuildUserWithRoleSql(UserQueries.GetActiveUsersWithRole, " u.email= @email");  
        return (await DbManager.ReadAsync<UserWithRoleEntity>(baseSql, new Dictionary<string, object>
        {
            {"@email", email}
        }, GlobalSchema.Name)).FirstOrDefault();
    }

    public async Task<IEnumerable<DelegationEntity>> GetActiveDelegationsGivenByUserIdAsync(Guid userId)
    {
        var userDelegations = await DbManager.ReadAsync<DelegationEntity>(UserQueries.GetActiveDelegationsGivenByUserId, new Dictionary<string, object>
        {
            {"@user_id", userId}
        }, GlobalSchema.Name);

        return userDelegations;
    }

    public async Task<bool> UpdateUserImageFromAzureLoginAsync(string imagePath, string userId, string schemaName)
    {
        var isUpdated = await DbManager.ExecuteNonQueryAsync(UserQueries.UpdateUserImageFromAzureLogin, new Dictionary<string, object>
        {
            {"@user_id", userId},
            {"@image_path", imagePath}
        }, schemaName);

        return isUpdated;
    }

 
    public async Task<(IEnumerable<UserWithRoleEntity>, int TotalCount)> GetAllUsersWithFilterAsync(UsersFilterRequest request)
    {
        var baseSql = UserQueries.GetAllUsersWithRole;
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: UserGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: UserGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: UserGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: UserGridFilterConfig.GetSearchColumns(),
            DbManager.Dialect
        );
        var data = await DbManager.ReadAsync<UserWithRoleEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();
        return (data, count?.Count ?? 0);
    }

    public async Task<UserEntity?> UpdateUserAsync(UserEntity user)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@id", user.Id},
            {"@first_name", user.FirstName},
            {"@last_name", user.LastName},
            {"@email", user.Email},
            {"@status", user.Status},
            {"@updated_at", user.UpdatedAt},
            {"@updated_by", user.UpdatedBy}
        };

        var result = await DbManager.ReadAsync<UserEntity>(UserQueries.UpdateUser, parameters, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<bool> SoftDeleteUserAsync(Guid id, string reason)
    {
        var parameters = new Dictionary<string, object>
        {
            {"@id", id},
            {"@deleted_at", DateTime.UtcNow},
            {"@deleted_by", Guid.Empty},
            {"@delete_reason", reason}
        };
        var affected = await DbManager.ExecuteNonQueryAsync(UserQueries.SoftDeleteUser, parameters, GlobalSchema.Name);
        return affected;
    }
}
