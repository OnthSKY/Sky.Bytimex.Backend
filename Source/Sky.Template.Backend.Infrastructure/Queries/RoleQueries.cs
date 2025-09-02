
namespace Sky.Template.Backend.Infrastructure.Queries;

public class RoleQueries
{
    #region Get
    internal const string GetAllRolesWithUserCount = @"
        SELECT 
            r.id,
            r.name,
            r.description,
            r.status,
            r.created_at,
            COUNT(ur.user_id) as total_user_count
        FROM sys.roles r
        LEFT JOIN sys.user_roles ur ON ur.role_id = r.id
        WHERE r.is_deleted = FALSE
        GROUP BY r.id, r.name, r.description, r.status, r.created_at";

    internal const string GetRoleById = @"
        SELECT 
            r.id,
            r.name,
            r.description,
            r.status,
            r.created_at,
            COUNT(ur.user_id) as total_user_count
        FROM sys.roles r
        LEFT JOIN sys.user_roles ur ON ur.role_id = r.id
        WHERE r.id = @id AND r.is_deleted = FALSE
        GROUP BY r.id, r.name, r.description, r.status, r.created_at";

    internal const string GetFilteredPaginatedUsersByRoleId = @"
        SELECT 
            u.id,
            u.first_name,
            u.last_name,
            u.email,
            u.image_path
        FROM sys.users u
        INNER JOIN sys.user_roles ur ON ur.user_id = u.id
        WHERE ur.role_id = @roleId AND u.is_deleted = FALSE";
    #endregion

    #region Create
    internal const string CreateRole = @"
        INSERT INTO sys.roles (id, name, description, status, created_at, created_by)
        VALUES (@id, @name, @description, @status, @createdAt, @createdBy)
        RETURNING *";
    #endregion

    #region Update
    internal const string UpdateRole = @"
        UPDATE sys.roles 
        SET name = @name, description = @description, status = @status, updated_at = @updatedAt, updated_by = @updatedBy
        WHERE id = @id AND is_deleted = FALSE
        RETURNING *";

    internal const string UpdateUserRole = @"
        UPDATE sys.user_roles
        SET role_id = @roleId, updated_at = @updatedAt, updated_by = @updatedBy
        WHERE user_id = @userId";

    internal const string AddPermissionToRole = @"
        INSERT INTO sys.role_permissions(role_id, permission_id, created_at, created_by)
        VALUES (@roleId, @permissionId, @createdAt, @createdBy)
        ON CONFLICT (role_id, permission_id) DO NOTHING";
    #endregion

    #region Delete
    internal const string DeleteRole = @"
        DELETE FROM sys.roles WHERE id = @id";

    internal const string SoftDeleteRole = @"
        UPDATE sys.roles 
        SET is_deleted = TRUE, deleted_at = @deletedAt, deleted_by = @deletedBy, delete_reason = @deleteReason
        WHERE id = @id";
    #endregion
}

