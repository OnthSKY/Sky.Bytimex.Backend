using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminReferralService
{
    Task<BaseControllerResponse> GetReferralTreeAsync();
}
