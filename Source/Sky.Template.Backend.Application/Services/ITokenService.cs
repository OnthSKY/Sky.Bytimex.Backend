using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sky.Template.Backend.Contract.Responses.UserResponses;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Sky.Template.Backend.Application.Services.User;

namespace Sky.Template.Backend.Application.Services;

public interface ITokenService
{

    string GenerateRefreshToken();
    Task<(string, DateTime)> GenerateJwtTokenByEmail(string email);
    //Task<(string, DateTime)> GenerateJwtToken(string userId, string userFirstName, string userLastName, string userEmail, string userImage, string role, string schema);
    Task<List<Claim>> GetTokenClaimsByEmailForUser(string email, Guid impersonatedUserId = default);
    Task<List<Claim>> GetTokenClaimsByUserIdForUser(Guid userId, Guid impersonatedUserId = default);

}

public class TokenService : ITokenService
{
    private readonly IPasswordHashService _passwordHashService;
    private readonly IUserService _userService;
    private readonly TokenManagerConfig _tokenManagerConfig;

    public TokenService(IPasswordHashService passwordHashService, IOptions<TokenManagerConfig> tokenManagerConfig, IUserService userService)
    {
        _passwordHashService = passwordHashService;
        _tokenManagerConfig = tokenManagerConfig.Value;
        _userService = userService;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<(string, DateTime)> GenerateJwtTokenByEmail(string userEmail)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_tokenManagerConfig.Secret);
        DateTime expireDate = DateTime.UtcNow.AddMinutes(_tokenManagerConfig.AccessTokenExpireMinutes);

        var claims = await GetTokenClaimsByEmailForUser(userEmail);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expireDate,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _tokenManagerConfig.Issuer,
            Audience = _tokenManagerConfig.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expireDate);
    }

    public async Task<List<Claim>> GetTokenClaimsByUserIdForUser(Guid userId, Guid impersonatedUserId = default)
    {
        var response = await _userService.GetUserDtoByIdOrThrowAsync(userId);
        return BuildTokenClaims(response.Data.User, impersonatedUserId);
    }

    public async Task<List<Claim>> GetTokenClaimsByEmailForUser(string email, Guid impersonatedUserId = default)
    {
        var response = await _userService.GetUserDtoByEmailOrThrowAsync(email);
        return BuildTokenClaims(response.Data.User, impersonatedUserId);
    }

    private List<Claim> BuildTokenClaims(UserWithRoleDto user, Guid impersonatedUserId)
    {
        if (impersonatedUserId != GlobalImpersonationContext.GetSentinelValue())
            GlobalImpersonationContext.AdminId = impersonatedUserId;

        if (impersonatedUserId == GlobalImpersonationContext.GetRemovalSentinelValue())
            GlobalImpersonationContext.AdminId = GlobalImpersonationContext.GetSentinelValue();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Uri, user.UserImagePath ?? ""),
            new Claim(ClaimTypes.Actor, user.UserImagePath ?? ""),
            new Claim(ClaimTypes.GroupSid, GlobalSchema.Name),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim(CustomClaimTypes.ImpersonatedBy, GlobalImpersonationContext.IsImpersonating
                ? GlobalImpersonationContext.AdminId.ToString()
                : GlobalImpersonationContext.GetSentinelValue().ToString())
        };

        if (user.Role.PermissionList is { Count: > 0 })
        {
            claims.AddRange(user.Role.PermissionList.Select(p => new Claim(CustomClaimTypes.Permission, p)));
        }

        return claims;
    }
}
