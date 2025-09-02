using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Contract.Responses.SettingResponses;

namespace Sky.Template.Backend.Infrastructure.Repositories.System;

public interface ISettingRepository
{
    Task<SystemSettingEntity?> GetGlobalSettingAsync(string key);
    Task<IEnumerable<SystemSettingEntity>> GetAllGlobalSettingsAsync();
    Task<IEnumerable<SettingDto>> GetVendorSettingsAsync(Guid vendorId);
    Task<VendorSettingEntity?> GetVendorSettingAsync(Guid vendorId, string key);
    Task UpsertGlobalSettingAsync(SystemSettingEntity entity, DbConnection? connection = null, DbTransaction? transaction = null);
    Task UpsertVendorSettingAsync(VendorSettingEntity entity, DbConnection? connection = null, DbTransaction? transaction = null);
    Task<IDictionary<string, string>> GetSettingsAsync(IEnumerable<string> keys);
}

public class SettingRepository : ISettingRepository
{
    public async Task<SystemSettingEntity?> GetGlobalSettingAsync(string key)
    {
        var sql = "SELECT * FROM sys.system_settings WHERE key = @key AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<SystemSettingEntity>(sql, new Dictionary<string, object>{{"@key", key}}, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<SystemSettingEntity>> GetAllGlobalSettingsAsync()
    {
        var sql = "SELECT * FROM sys.system_settings WHERE is_deleted = FALSE ORDER BY key";
        return await DbManager.ReadAsync<SystemSettingEntity>(sql, new Dictionary<string, object>(), GlobalSchema.Name);
    }

    public async Task<IEnumerable<SettingDto>> GetVendorSettingsAsync(Guid vendorId)
    {
        var sql = @"SELECT vs.key, ss.group_name AS Group, vs.value, ss.description, ss.is_public
                     FROM sys.vendor_settings vs
                     INNER JOIN sys.system_settings ss ON ss.key = vs.key
                     WHERE vs.vendor_id = @vendorId AND vs.is_deleted = FALSE";
        var parameters = new Dictionary<string, object>{{"@vendorId", vendorId}};
        return await DbManager.ReadAsync<SettingDto>(sql, parameters, GlobalSchema.Name);
    }

    public async Task<VendorSettingEntity?> GetVendorSettingAsync(Guid vendorId, string key)
    {
        var sql = "SELECT * FROM sys.vendor_settings WHERE vendor_id = @vendorId AND key = @key AND is_deleted = FALSE";
        var parameters = new Dictionary<string, object>{{"@vendorId", vendorId}, {"@key", key}};
        var result = await DbManager.ReadAsync<VendorSettingEntity>(sql, parameters, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<IDictionary<string, string>> GetSettingsAsync(IEnumerable<string> keys)
    {
        var sql = "SELECT * FROM sys.system_settings WHERE key = ANY(@keys) AND is_deleted = FALSE";
        var parameters = new Dictionary<string, object> { { "@keys", keys.ToArray() } };
        var rows = await DbManager.ReadAsync<SystemSettingEntity>(sql, parameters, GlobalSchema.Name);
        return rows.ToDictionary(r => r.Key, r => r.Value);
    }

    public async Task UpsertGlobalSettingAsync(SystemSettingEntity entity, DbConnection? connection = null, DbTransaction? transaction = null)
    {
        var sql = @"INSERT INTO sys.system_settings (key, group_name, value, description, is_public, created_at, created_by, updated_at, updated_by, is_deleted)
                    VALUES (@key, @groupName, @value, @description, @isPublic, @createdAt, @createdBy, @updatedAt, @updatedBy, FALSE)
                    ON CONFLICT (key) DO UPDATE SET value = EXCLUDED.value, updated_at = EXCLUDED.updated_at, updated_by = EXCLUDED.updated_by";
        var parameters = new Dictionary<string, object>
        {
            {"@key", entity.Key},
            {"@groupName", entity.Group},
            {"@value", entity.Value},
            {"@description", entity.Description},
            {"@isPublic", entity.IsPublic},
            {"@createdAt", entity.CreatedAt},
            {"@createdBy", entity.CreatedBy},
            {"@updatedAt", entity.UpdatedAt},
            {"@updatedBy", entity.UpdatedBy}
        };
        if (connection != null && transaction != null)
            await DbManager.ExecuteTransactionNonQueryWithAsync(sql, parameters, connection, transaction, GlobalSchema.Name);
        else
            await DbManager.ExecuteNonQueryAsync(sql, parameters, GlobalSchema.Name);
    }

    public async Task UpsertVendorSettingAsync(VendorSettingEntity entity, DbConnection? connection = null, DbTransaction? transaction = null)
    {
        var sql = @"INSERT INTO sys.vendor_settings (vendor_id, key, value, created_at, created_by, updated_at, updated_by, is_deleted)
                    VALUES (@vendorId, @key, @value, @createdAt, @createdBy, @updatedAt, @updatedBy, FALSE)
                    ON CONFLICT (vendor_id, key) DO UPDATE SET value = EXCLUDED.value, updated_at = EXCLUDED.updated_at, updated_by = EXCLUDED.updated_by";
        var parameters = new Dictionary<string, object>
        {
            {"@vendorId", entity.VendorId},
            {"@key", entity.Key},
            {"@value", entity.Value},
            {"@createdAt", entity.CreatedAt},
            {"@createdBy", entity.CreatedBy},
            {"@updatedAt", entity.UpdatedAt},
            {"@updatedBy", entity.UpdatedBy}
        };
        if (connection != null && transaction != null)
            await DbManager.ExecuteTransactionNonQueryWithAsync(sql, parameters, connection, transaction, GlobalSchema.Name);
        else
            await DbManager.ExecuteNonQueryAsync(sql, parameters, GlobalSchema.Name);
    }
}
