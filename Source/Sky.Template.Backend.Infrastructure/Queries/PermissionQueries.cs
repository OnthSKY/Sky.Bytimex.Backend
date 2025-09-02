namespace Sky.Template.Backend.Infrastructure.Queries;

public class PermissionQueries
{
    #region Get
    internal const string GetAllPermissions = @"
        SELECT * FROM sys.permissions 
        WHERE is_deleted = FALSE";

    internal const string GetPermissionById = @"
        SELECT * FROM sys.permissions 
        WHERE id = @id AND is_deleted = FALSE";
    #endregion

    #region Create
    internal const string CreatePermission = @"
        INSERT INTO sys.permissions (id, name, description, created_at, created_by)
        VALUES (@id, @name, @description, @createdAt, @createdBy)
        RETURNING *";
    #endregion

    #region Update
    internal const string UpdatePermission = @"
        UPDATE sys.permissions 
        SET name = @name, description = @description, updated_at = @updatedAt, updated_by = @updatedBy
        WHERE id = @id AND is_deleted = FALSE
        RETURNING *";
    #endregion

    #region Delete
    internal const string DeletePermission = @"
        DELETE FROM sys.permissions WHERE id = @id";

    internal const string SoftDeletePermission = @"
        UPDATE sys.permissions 
        SET is_deleted = TRUE, deleted_at = @deletedAt, deleted_by = @deletedBy, delete_reason = @deleteReason
        WHERE id = @id";
    #endregion
} 