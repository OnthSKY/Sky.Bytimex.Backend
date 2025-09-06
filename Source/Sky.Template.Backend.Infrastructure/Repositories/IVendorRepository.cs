using Sky.Template.Backend.Infrastructure.Entities.Vendor;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Configs.Vendors;
using System.Collections.Generic;
using System.Data.Common;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IVendorRepository : IRepository<VendorEntity, Guid>
{
    Task<VendorEntity?> GetVendorByEmailAsync(string email);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<string?> GetKycStatusAsync(Guid vendorId, DbConnection connection, DbTransaction transaction);
}

public class VendorRepository : Repository<VendorEntity, Guid>, IVendorRepository
{
    public override async Task<(IEnumerable<VendorEntity>, int TotalCount)> GetFilteredPaginatedAsync(GridRequest request)
    {
        var baseSql = "SELECT * FROM sys.vendors v WHERE v.is_deleted = FALSE";
        var (sql, parameters) = GridQueryBuilder.Build(
            baseSql: baseSql,
            request: request,
            columnMappings: VendorGridFilterConfig.GetColumnMappings(),
            defaultOrderBy: VendorGridFilterConfig.GetDefaultOrder(),
            likeFilterKeys: VendorGridFilterConfig.GetLikeFilterKeys(),
            searchColumns: VendorGridFilterConfig.GetSearchColumns(),
            DbManager.Dialect
        );

        var data = await DbManager.ReadAsync<VendorEntity>(sql, parameters, GlobalSchema.Name);
        var countSql = DbManager.Dialect.CountWrap(DbManager.Dialect.StripOrderBy(sql));
        var count = (await DbManager.ReadAsync<DataCountEntity>(countSql, parameters, GlobalSchema.Name)).FirstOrDefault();

        return (data, count?.Count ?? 0);
    }

    public async Task<VendorEntity?> GetVendorByEmailAsync(string email)
    {
        var sql = "SELECT * FROM sys.vendors WHERE email = @email AND is_deleted = FALSE";
        var result = await DbManager.ReadAsync<VendorEntity>(sql, new Dictionary<string, object> { { "@email", email } }, GlobalSchema.Name);
        return result.FirstOrDefault();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        var sql = excludeId.HasValue
            ? "SELECT COUNT(*) FROM sys.vendors WHERE email = @email AND id != @excludeId AND is_deleted = FALSE"
            : "SELECT COUNT(*) FROM sys.vendors WHERE email = @email AND is_deleted = FALSE";

        var parameters = new Dictionary<string, object> { { "@email", email } };
        if (excludeId.HasValue)
            parameters.Add("@excludeId", excludeId.Value);

        var count = await DbManager.ReadAsync<DataCountEntity>(sql, parameters, GlobalSchema.Name);
        return count.FirstOrDefault()?.Count == 0;
    }

    public async Task<string?> GetKycStatusAsync(Guid vendorId, DbConnection connection, DbTransaction transaction)
    {
        const string vendorSql = "SELECT kyc_status FROM sys.vendors WHERE id = @id";
        var vendorResult = await DbManager.ReadAsync<KycStatusEntity>(vendorSql, new Dictionary<string, object> { { "@id", vendorId } }, connection, transaction, GlobalSchema.Name);
        var status = vendorResult.FirstOrDefault()?.KycStatus;
        if (status != null)
            return status;

        const string ownerSql = "SELECT u.kyc_status FROM sys.vendor_users vu JOIN sys.users u ON vu.user_id = u.id WHERE vu.vendor_id = @vid ORDER BY vu.is_owner DESC LIMIT 1";
        var ownerResult = await DbManager.ReadAsync<KycStatusEntity>(ownerSql, new Dictionary<string, object> { { "@vid", vendorId } }, connection, transaction, GlobalSchema.Name);
        return ownerResult.FirstOrDefault()?.KycStatus;
    }

    private class KycStatusEntity
    {
        public string? KycStatus { get; set; }
    }
}
