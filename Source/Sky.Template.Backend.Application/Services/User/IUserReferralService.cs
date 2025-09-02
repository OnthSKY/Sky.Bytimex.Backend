using System;
using Sky.Template.Backend.Core.BaseResponse;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserReferralService
{
    Task<BaseControllerResponse> GetOwnReferralsAsync(Guid userId);
}
