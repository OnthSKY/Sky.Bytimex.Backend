using System.Collections.Generic;
using System.Linq;
using Sky.Template.Backend.Infrastructure.Entities.Kyc;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Core.Context;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IVendorKycRepository : IRepository<VendorKycEntity, Guid>
{
    Task<VendorKycEntity?> GetByVendorIdAsync(Guid vendorId);
}

public class VendorKycRepository : Repository<VendorKycEntity, Guid>, IVendorKycRepository
{
    public async Task<VendorKycEntity?> GetByVendorIdAsync(Guid vendorId)
    {
        var sql = "SELECT * FROM sys.vendor_kyc_submissions WHERE vendor_id = @vid";
        var data = await DbManager.ReadAsync<VendorKycEntity>(sql, new Dictionary<string, object> { {"@vid", vendorId} }, GlobalSchema.Name);
        return data.FirstOrDefault();
    }
}
