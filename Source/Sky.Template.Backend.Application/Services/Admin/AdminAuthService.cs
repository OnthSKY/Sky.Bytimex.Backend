using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Contract.Responses.Auth;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminAuthService
{
    Task<BaseControllerResponse<AuthResponse>> ImpersonateAsync(ImpersonateRequest request);
    Task<BaseControllerResponse<AuthResponse>> ReturnUserAsync();
    Task<BaseControllerResponse<AuthResponse>> LoginWithADAsync(string? code, string? error, string? errorDescription);
}

public class AdminAuthService : SharedAuthService, IAdminAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AzureAdLoginConfig _azureAdLoginConfig;

    public AdminAuthService(
        IAuthRepository authRepository,
        IOptions<TokenManagerConfig> tokenOptions,
        IUnitOfWork unitOfWork,
        IPasswordHashService passwordService,
        ITokenService tokenService,
        IUserService userService,
        IHttpContextAccessor httpContextAccessor,
        IReferralRewardRepository referralRepository,
        IOptions<AzureAdLoginConfig> azureAdLoginConfig)
        : base(authRepository, tokenOptions, unitOfWork, passwordService, tokenService, userService, referralRepository)
    {
        _tokenService = tokenService;
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _azureAdLoginConfig = azureAdLoginConfig.Value;
    }

    public async Task<BaseControllerResponse<AuthResponse>> ImpersonateAsync(ImpersonateRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            throw new UnAuthorizedException("UserEmail.IsRequired");
        }

        var context = _httpContextAccessor.HttpContext ?? throw new UnAuthorizedException("ContextNotFound");

        if (!GlobalImpersonationContext.IsImpersonating)
        {
            GlobalImpersonationContext.AdminId = context.GetUserId();
        }

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        var claims = await _tokenService.GetTokenClaimsByEmailForUser(request.Email);
        identity.AddClaims(claims);
        var principal = new ClaimsPrincipal(identity);

        await LogoutInternalAsync();
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var response = await AuthenticateByEmailAsync(new AuthWithoutPasswordRequest { Username = request.Email });

        if (response.Data is not null)
        {
            context.Response.Cookies.Append("refresh_token", response.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = response.Data.RefreshExpireDate
            });

            context.Response.Cookies.Append("access_token", response.Data.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = response.Data.TokenExpireDate
            });
        }

        return response;
    }

    public async Task<BaseControllerResponse<AuthResponse>> ReturnUserAsync()
    {
        var context = _httpContextAccessor.HttpContext ?? throw new UnAuthorizedException("ContextNotFound");

        if (GlobalImpersonationContext.AdminId == context.GetUserId())
        {
            throw new BusinessRulesException("UserAlreadySelfUser");
        }

        var userInfoResponse = await _userService.GetUserDtoByIdOrThrowAsync(GlobalImpersonationContext.AdminId);

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        var userClaims = await _tokenService.GetTokenClaimsByUserIdForUser(userInfoResponse.Data.User.Id);
        identity.AddClaims(userClaims);
        var principal = new ClaimsPrincipal(identity);

        await LogoutInternalAsync();

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var response = await AuthenticateByEmailAsync(new AuthWithoutPasswordRequest
        {
            Username = userInfoResponse.Data.User.Email
        });

        if (response.Data is not null)
        {
            context.Response.Cookies.Append("refresh_token", response.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = response.Data.RefreshExpireDate
            });

            context.Response.Cookies.Append("access_token", response.Data.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = response.Data.TokenExpireDate
            });
        }

        return response;
    }

    public async Task<BaseControllerResponse<AuthResponse>> LoginWithADAsync(string? code, string? error, string? errorDescription)
    {
        if (!string.IsNullOrEmpty(error))
        {
            throw new BusinessRulesException(errorDescription ?? "UnknownError");
        }

        if (!string.IsNullOrEmpty(code))
        {
            string tokenEndpoint = $"https://login.microsoftonline.com/{_azureAdLoginConfig.TenantId}/oauth2/v2.0/token";
            var tokenRequestParams = new Dictionary<string, string>
            {
                { "client_id", _azureAdLoginConfig.ClientId },
                { "scope", "openid profile email" },
                { "code", code },
                { "redirect_uri", _azureAdLoginConfig.RedirectUrl },
                { "grant_type", "authorization_code" },
                { "client_secret", _azureAdLoginConfig.ClientSecret }
            };

            using var httpClient = new HttpClient();
            var requestContent = new FormUrlEncodedContent(tokenRequestParams);
            var response = await httpClient.PostAsync(tokenEndpoint, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse =  JsonDocument.Parse(responseContent).RootElement;
                var errorDesc = errorResponse.GetProperty("error_description").GetString();
                throw new BusinessRulesException(errorDesc);
            }

            var tokenResponse =   JsonDocument.Parse(responseContent).RootElement;
            var idToken = tokenResponse.GetProperty("id_token").GetString();
            var accessToken = tokenResponse.GetProperty("access_token").GetString();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(idToken);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");

            if (emailClaim is null)
            {
                throw new UnAuthorizedException("UserNotFound");
            }

            using var httpClientUserImage = new HttpClient();
            httpClientUserImage.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var graphPhotoEndpoint = "https://graph.microsoft.com/v1.0/me/photo/$value";
            var responseUserImage = await httpClientUserImage.GetAsync(graphPhotoEndpoint);

            var userInfoResponseModel = await AuthenticateByEmailAsync(new AuthWithoutPasswordRequest { Username = emailClaim.Value });
            if (responseUserImage.IsSuccessStatusCode)
            {
                var photoBytes = await responseUserImage.Content.ReadAsByteArrayAsync();
                var imageUrl = await _userService.UploadUserImageToAzureBlobStorageAsync(photoBytes,
                    userInfoResponseModel.Data.User.Id.ToString(), $"{userInfoResponseModel.Data.User.FirstName}_{userInfoResponseModel.Data.User.LastName}");
                await _userService.UpdateUserImageFromAzureLoginAsync(imageUrl,
                    userInfoResponseModel.Data.User.Id.ToString(),
                    userInfoResponseModel.Data.User.SchemaName);
            }

            var context = _httpContextAccessor.HttpContext!;
            context.Response.Cookies.Append("refresh_token", userInfoResponseModel.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = userInfoResponseModel.Data.RefreshExpireDate
            });

            context.Response.Cookies.Append("access_token", userInfoResponseModel.Data.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = userInfoResponseModel.Data.TokenExpireDate
            });

            return userInfoResponseModel;
        }

        throw new UnAuthorizedException("UserNotFound");
    }

    private async Task LogoutInternalAsync()
    {
        var context = _httpContextAccessor.HttpContext ?? throw new UnAuthorizedException("ContextNotFound");
        var refreshToken = context.Request.Cookies["refresh_token"];

        if (string.IsNullOrEmpty(refreshToken))
            refreshToken = context.Request.Headers["x-refresh-token"];
        if (string.IsNullOrEmpty(refreshToken) && context.Request.HasFormContentType)
            refreshToken = context.Request.Form["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnAuthorizedException("RefreshTokenNotFound");

        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        await LogoutAsync(refreshToken, clientIp);

        if (context.Request.Cookies.ContainsKey("refresh_token"))
        {
            context.Response.Cookies.Delete("refresh_token");
            context.Response.Cookies.Delete("access_token");
        }
    }
}
