namespace Sky.Template.Backend.Infrastructure.Queries;

public class AuthQueries
{
    #region Read-Queries

    internal const string GetActiveAccountUsers = @"SELECT * FROM $db.users WHERE status='ACTIVE' ";

    internal const string GetUserWithRole = @"SELECT 
                                            u.id,u.first_name, u.last_name, u.email, u.image_path, r.name as role_name, r.description  as role_description, u.status as status,
                                    STRING_AGG(p.name, ',') AS permission_names
                                            FROM $db.users as u
                                             INNER JOIN $db.user_roles AS ur ON ur.user_id = u.id
                                            INNER JOIN $db.roles AS r ON r.id = ur.role_id
                                            LEFT JOIN $db.role_permissions AS rp ON rp.role_id = r.id
                                            LEFT JOIN $db.permissions AS p ON p.id = rp.permission_id
								 
                                         WHERE  u.email = @user_email /**extra_where**/
                                GROUP BY u.id, u.first_name, u.last_name, u.email, u.image_path, r.id, r.name, r.description, u.status ;";

    internal const string GetRefreshToken = @"SELECT * FROM $db.refresh_tokens WHERE token = @token AND revoked_at IS NULL";

    internal const string CountActiveTokens = @"SELECT COUNT(*) as 'count' FROM $db.refresh_tokens WHERE email = @user_email
                                                  AND schema_name = @schema_name
                                                  AND revoked_at IS NULL
                                                  AND expiration_date > CURRENT_TIMESTAMP";

    // internal const string GetSchemaByInviteCode = @"
    //     SELECT t.schema_name 
    //     FROM public.tenant_invite_codes tic
    //     INNER JOIN public.tenants t ON t.id = tic.tenant_id
    //     WHERE tic.invite_code = @invite_code 
    //     AND tic.is_active = true 
    //     AND tic.expires_at > CURRENT_TIMESTAMP";
    //
    // internal const string ValidateInviteCode = @"
    //     SELECT COUNT(*) as count 
    //     FROM public.tenant_invite_codes tic
    //     INNER JOIN public.tenants t ON t.id = tic.tenant_id
    //     WHERE tic.invite_code = @invite_code 
    //     AND tic.is_active = true 
    //     AND tic.expires_at > CURRENT_TIMESTAMP";

    // internal const string GetInviteCodesByTenantId = @"
    //     SELECT * FROM public.tenant_invite_codes 
    //     WHERE tenant_id = @tenant_id 
    //     ORDER BY created_at DESC";

    // internal const string GetInviteCodeById = @"
    //     SELECT * FROM public.tenant_invite_codes 
    //     WHERE id = @id";
    //
    // internal const string DeactivateInviteCode = @"
    //     UPDATE public.tenant_invite_codes 
    //     SET is_active = false 
    //     WHERE id = @id";

    // internal const string CreateInviteCode = @"
    //     INSERT INTO public.tenant_invite_codes (
    //         id,
    //         tenant_id,
    //         invite_code,
    //         is_active,
    //         created_at,
    //         expires_at,
    //         created_by
    //     )
    //     VALUES (
    //         @id,
    //         @tenant_id,
    //         @invite_code,
    //         true,
    //         CURRENT_TIMESTAMP,
    //         @expires_at,
    //         @created_by
    //     )";

    #endregion

    #region Transaction-Queries

    internal const string InsertRefreshToken = @"
    INSERT INTO sys.refresh_tokens (
        id,
        user_id,
        token,
        expiration_date,
        created_at,
        created_by_ip,
        schema_name
    )
    VALUES (
        @id,
        @user_id,
        @token,
        @expiration_date,
        @created_at,
        @created_by_ip,
        @schema_name
    )";

    internal const string UpdateLastLoginDate = @"UPDATE sys.users SET last_login_date = NOW() WHERE email = @email; ";

    internal const string RevokeOldestActiveToken = @"
    UPDATE sys.refresh_tokens
    SET revoked_at = NOW()
    WHERE id = (
        SELECT TOP 1 id FROM sys.refresh_tokens
        WHERE email = @user_email
          AND schema_name = @schema_name
          AND revoked_at IS NULL
          AND expiration_date > CURRENT_TIMESTAMP
        ORDER BY created_at ASC
    )
";

    internal const string RevokeRefreshToken = @"
    UPDATE sys.refresh_tokens
    SET 
        revoked_at = NOW(),
        revoked_by_ip = @revoked_by_ip
        WHERE token = @token
      AND revoked_at IS NULL
";

    internal const string InsertUser = @"
    INSERT INTO sys.users (
        id,
        email,
        phone,
        password_hash,
        created_at,
        tenant_schema_name,
        status
    )
    VALUES (
        @id,
        @email,
        @phone,
        @password,
        @created_at,
        @tenant_schema_name,
        @status
    );";

    internal const string InsertUserToSchema = @"
            INSERT INTO $db.users (
                id,
                username,
                first_name,
                last_name,
                email,
                password_hash,
                phone,
                preferred_language,
                status,
                referred_by,
                created_at
            )
            VALUES (
                @id,
                @username,
                @first_name,
                @last_name,
                @email,
                @password_hash,
                @phone,
                @preferred_language,
                CAST(@status AS status_enum),
                @referred_by,
                @created_at
            )";


    internal const string AddUserToRoleByRoleName = @"insert into $db.user_roles (user_id, role_id) values (@id, (select id from $db.roles where NAME=@role_name))";  

    #endregion
}
