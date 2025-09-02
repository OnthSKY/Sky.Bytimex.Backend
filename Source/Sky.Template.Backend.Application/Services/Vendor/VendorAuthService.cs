using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Contract.Responses.Auth;
using Sky.Template.Backend.Application.Services;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorAuthService
{
    Task<BaseControllerResponse<AuthResponse>> RegisterVendorAsync(RegisterVendorRequest request);
}

public class VendorAuthService : IVendorAuthService
{
    private readonly ISharedAuthService _sharedAuthService;

    public VendorAuthService(ISharedAuthService sharedAuthService)
    {
        _sharedAuthService = sharedAuthService;
    }

    public Task<BaseControllerResponse<AuthResponse>> RegisterVendorAsync(RegisterVendorRequest request)
        => _sharedAuthService.RegisterVendorAsync(request);
}

