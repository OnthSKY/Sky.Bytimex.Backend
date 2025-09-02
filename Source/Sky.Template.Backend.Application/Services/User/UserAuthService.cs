using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Contract.Responses.Auth;
using Sky.Template.Backend.Application.Services;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserAuthService
{
    Task<BaseControllerResponse<AuthResponse>> RegisterAsync(RegisterUserRequest request);
}

public class UserAuthService : IUserAuthService
{
    private readonly ISharedAuthService _sharedAuthService;

    public UserAuthService(ISharedAuthService sharedAuthService)
    {
        _sharedAuthService = sharedAuthService;
    }

    public Task<BaseControllerResponse<AuthResponse>> RegisterAsync(RegisterUserRequest request)
        => _sharedAuthService.RegisterUserAsync(request);
}

