using System;
using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorReferralService
{
    Task<BaseControllerResponse> GetVendorReferralsAsync(Guid vendorId);
}
