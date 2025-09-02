using System;
using System.Collections.Generic;
using System.Linq;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IVendorKycVerificationRepository : IRepository<VendorKycVerificationEntity, long>
{
    Task<VendorKycVerificationEntity?> GetByVendorIdAsync(long vendorId);
    Task UpsertAsync(VendorKycVerificationEntity entity);
    Task ReviewAsync(long vendorId, string status, long? reviewedBy, string? notes);
}

public class VendorKycVerificationRepository : Repository<VendorKycVerificationEntity, long>, IVendorKycVerificationRepository
{
    public async Task<VendorKycVerificationEntity?> GetByVendorIdAsync(long vendorId)
    {
        const string sql = "SELECT * FROM sys.vendor_kyc_verifications WHERE vendor_id = @vid";
        var data = await DbManager.ReadAsync<VendorKycVerificationEntity>(sql, new Dictionary<string, object> { {"@vid", vendorId} }, GlobalSchema.Name);
        return data.FirstOrDefault();
    }

    public async Task UpsertAsync(VendorKycVerificationEntity entity)
    {
        const string sql = @"INSERT INTO sys.vendor_kyc_verifications (vendor_id, status, notes, reviewed_by, reviewed_at)
                             VALUES (@vendor_id, @status, @notes, @reviewed_by, @reviewed_at)
                             ON CONFLICT (vendor_id) DO UPDATE
                             SET status = EXCLUDED.status,
                                 notes = EXCLUDED.notes,
                                 reviewed_by = EXCLUDED.reviewed_by,
                                 reviewed_at = EXCLUDED.reviewed_at";
        var parameters = new Dictionary<string, object>
        {
            {"@vendor_id", entity.VendorId},
            {"@status", entity.Status},
            {"@notes", entity.Notes ?? (object)DBNull.Value},
            {"@reviewed_by", entity.ReviewedBy ?? (object)DBNull.Value},
            {"@reviewed_at", entity.ReviewedAt ?? (object)DBNull.Value}
        };
        await DbManager.ExecuteNonQueryAsync(sql, parameters, GlobalSchema.Name);
    }

    public async Task ReviewAsync(long vendorId, string status, long? reviewedBy, string? notes)
    {
        const string sql = @"UPDATE sys.vendor_kyc_verifications
                             SET status = @status,
                                 reviewed_by = @reviewed_by,
                                 reviewed_at = @reviewed_at,
                                 notes = @notes
                             WHERE vendor_id = @vendor_id";
        var parameters = new Dictionary<string, object>
        {
            {"@status", status},
            {"@reviewed_by", reviewedBy ?? (object)DBNull.Value},
            {"@reviewed_at", DateTime.UtcNow},
            {"@notes", notes ?? (object)DBNull.Value},
            {"@vendor_id", vendorId}
        };
        await DbManager.ExecuteTransactionalNonQueryAsync(sql, parameters);
    }
}
