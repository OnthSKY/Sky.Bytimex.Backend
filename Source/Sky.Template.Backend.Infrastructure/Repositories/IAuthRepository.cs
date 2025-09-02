using System.Data.Common;
using System.Text;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.Auth;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Infrastructure.Queries;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;


public interface IAuthRepository
{
    #region Read-Interfaces

    Task<RefreshTokenEntity?> GetRefreshTokenAsync(string token);
    Task<AccountUserEntity> GetSystemUserByIdAndSchemaAsync(Guid userId);
    Task<AccountUserEntity> GetSystemUserByEmailAsync(string email);
    Task<AccountUserEntity> GetSystemUserByPhoneByAsync(string phone);
    Task<UserWithRoleEntity> GetWithRoleBySchemaAsync(string email, string userSchema);

    #endregion


    #region Transaction-Interfaces

    Task<bool> UpdateLastLoginDateAsync(string email, string userSchema);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? revokedByIp);
    Task<bool> InsertRefreshTokenAsync(Guid userId, string token, DateTime expirationDate, string schemaName, string? createdByIp);
    Task<bool> InsertRefreshTokenAsync(Guid userId, string token, DateTime expirationDate, string schemaName, string? createdByIp, DbConnection connection, DbTransaction transaction);
    Task<bool> UpdateLastLoginDateAsync(string email, string userSchema, DbConnection connection, DbTransaction transaction);
    Task<bool> RegisterUserAsync(Guid userId, string email, string phone, string hashedPassword, string schemaName, DbConnection connection, DbTransaction transaction);
    Task<bool> RegisterUserToSchemaAsync(Guid userId, string email, string passwordHash, string phone, string firstName, string lastName, string? preferredLanguage, Guid? referredBy, DbConnection connection, DbTransaction transaction);
    Task<bool> AddRoleToUserByRoleNameAsync(Guid userId, string roleName, DbConnection connection, DbTransaction transaction);

    #endregion


}
public class AuthRepository : IAuthRepository
{
    public async Task<DataCountEntity> GetActiveRefreshTokenCountAsync(string userEmail, string schemaName)
    {
        return (await DbManager.ReadAsync<DataCountEntity>(AuthQueries.CountActiveTokens, new Dictionary<string, object>
        {
            { "@user_email", userEmail },
            { "@schema_name", schemaName }
        })).FirstOrDefault();
    }



    public async Task<RefreshTokenEntity?> GetRefreshTokenAsync(string token)
    {
        var result = await DbManager.ReadAsync<RefreshTokenEntity>(
            AuthQueries.GetRefreshToken,
            new Dictionary<string, object>
            {
                { "@token", token }
            });

        return result.FirstOrDefault();
    }
    public async Task<AccountUserEntity> GetSystemUserByIdAndSchemaAsync(Guid userId)
    {
        var sql = new StringBuilder(AuthQueries.GetActiveAccountUsers);

        sql.AppendLine("and id = @user_id");

        return (await DbManager.ReadAsync<AccountUserEntity>(sql.ToString(), new Dictionary<string, object>()
        {
            { "@user_id",userId},
        })).FirstOrDefault();
    }
    public async Task<AccountUserEntity> GetSystemUserByEmailAsync(string email)
    {
        var sql = new StringBuilder(AuthQueries.GetActiveAccountUsers);

        if (!string.IsNullOrEmpty(email))
        {
            sql.AppendLine("AND email = @user_email");
        }

        return (await DbManager.ReadAsync<AccountUserEntity>(sql.ToString(), new Dictionary<string, object>()
        {
            { "@user_email",email}
        })).FirstOrDefault();
    }

    public async Task<AccountUserEntity> GetSystemUserByPhoneByAsync(string phone)
    {
        var sql = new StringBuilder(AuthQueries.GetActiveAccountUsers); ;

        if (!string.IsNullOrEmpty(phone))
        {
            sql.AppendLine("AND user_phone = @user_phone");
        }


        return (await DbManager.ReadAsync<AccountUserEntity>(sql.ToString(), new Dictionary<string, object>()
        {
            { "@user_phone",phone}
        })).FirstOrDefault();
    }

    public async Task<UserWithRoleEntity> GetWithRoleBySchemaAsync(string email, string userSchema)
    {
        return (await DbManager.ReadAsync<UserWithRoleEntity>(AuthQueries.GetUserWithRole, new Dictionary<string, object>
        {
            { "@user_email", email }
        }, userSchema)).FirstOrDefault();

    }

    public async Task<bool> UpdateLastLoginDateAsync(string email, string userSchema)
    {
        var isLastLoginDateUpdated = await DbManager.ExecuteNonQueryAsync(AuthQueries.UpdateLastLoginDate, new Dictionary<string, object>
        {
            { "@email", email }
        }, userSchema);

        return isLastLoginDateUpdated;
    }
    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? revokedByIp)
    {
        var result = await DbManager.ExecuteNonQueryAsync(AuthQueries.RevokeRefreshToken, new Dictionary<string, object>
        {
            { "@token", refreshToken },
            { "@revoked_by_ip", revokedByIp ?? string.Empty }
        });

        return result;
    }
    public async Task<bool> InsertRefreshTokenAsync(
        Guid userId,
        string token,
        DateTime expirationDate,
        string? schemaName,
        string? createdByIp)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@id", Guid.NewGuid() },
            { "@user_id", userId },
            { "@token", token },
            { "@expiration_date", expirationDate },
            { "@created_at", DateTime.Now },
            { "@created_by_ip", Utils.DbNullIfNull(createdByIp)},
            { "@schema_name", schemaName ?? "sys"}
        };

        return await DbManager.ExecuteNonQueryAsync(AuthQueries.InsertRefreshToken, parameters);
    }
    public async Task<bool> InsertRefreshTokenAsync(Guid userId, string token, DateTime expirationDate, string? schemaName, string? createdByIp, DbConnection connection, DbTransaction transaction)
    {
        var parameters = new Dictionary<string, object>
        {
            { "@id", Guid.NewGuid() },
            { "@user_id", userId },
            { "@token", token },
            { "@expiration_date", expirationDate },
            { "@created_at", DateTime.Now },
            { "@created_by_ip", Utils.DbNullIfNull(createdByIp)},
            { "@schema_name",  schemaName ?? "sys" }
        };

        return await DbManager.ExecuteTransactionNonQueryWithAsync(
            AuthQueries.InsertRefreshToken,
            parameters,
            connection,
            transaction,
            schemaName
        );
    }

    public async Task<bool> UpdateLastLoginDateAsync(
    string email,
    string userSchema,
    DbConnection connection,
    DbTransaction transaction)
    {
        var parameters = new Dictionary<string, object>
    {
        { "@email", email}
    };

        return await DbManager.ExecuteTransactionNonQueryWithAsync(
            AuthQueries.UpdateLastLoginDate,
            parameters,
            connection,
            transaction,
            userSchema
        );
    }

    public async Task<bool> RegisterUserAsync(Guid userId, string email, string phone, string hashedPassword, string schemaName, DbConnection connection, DbTransaction transaction)
    {
        // Insert into public.users
        var publicUserParameters = new Dictionary<string, object>
        {
            { "@id", userId },
            { "@email", email },
            { "@phone", phone },
            { "@password", hashedPassword },
            { "@created_at", DateTime.Now },
            { "@tenant_schema_name", schemaName },
            { "@status", 1 }
        };

        return await DbManager.ExecuteTransactionNonQueryWithAsync(
            AuthQueries.InsertUser,
            publicUserParameters,
            connection,
            transaction
        );


    }


    public async Task<bool> RegisterUserToSchemaAsync(Guid userId, string email, string passwordHash, string phone, string firstName, string lastName, string? preferredLanguage, Guid? referredBy, DbConnection connection, DbTransaction transaction)
    {
        var schemaUserParameters = new Dictionary<string, object>
        {
            { "@id", userId },
            { "username", email} ,//DEFAULT
            { "@first_name", firstName },
            { "@last_name", lastName },
            { "@email", email },
            { "@password_hash", passwordHash },
            { "@phone", phone },
            { "@preferred_language", Utils.DbNullIfNull(preferredLanguage) },
            { "@status", Status.ACTIVE.ToString()},
            { "@referred_by", Utils.DbNullIfNull(referredBy) },
            { "@created_at", DateTime.Now },
        };

        return await DbManager.ExecuteTransactionNonQueryWithAsync(
            AuthQueries.InsertUserToSchema,
            schemaUserParameters,
            connection,
            transaction,
            GlobalSchema.Name
        );
    }


    public async Task<bool> AddRoleToUserByRoleNameAsync(Guid userId, string roleName, DbConnection connection, DbTransaction transaction)
    {
        return await DbManager.ExecuteTransactionNonQueryWithAsync(
            AuthQueries.AddUserToRoleByRoleName,
            new Dictionary<string, object>()
            {
                {"@id", userId},
                {"@role_name", roleName }
            },
            connection,
            transaction, accountSchema: GlobalSchema.Name
        );
    }
}
