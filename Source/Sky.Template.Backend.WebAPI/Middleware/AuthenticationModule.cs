using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Sky.Template.Backend.Core.Configs;
using System.Text;
using Microsoft.Extensions.Localization;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.WebAPI.Middleware;
public static class AuthenticationModule
{
    public static void AddAuthorizationAndAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TokenManagerConfig>(configuration.GetSection("TokenConfigs"));

        var tokenConfig = configuration.GetSection("TokenConfigs").Get<TokenManagerConfig>();
        var key = Encoding.UTF8.GetBytes(tokenConfig.Secret);

        services.AddAuthorization(options =>
        {
            var defaultPolicy = new AuthorizationPolicyBuilder("SmartScheme")
                .RequireAuthenticatedUser()
                .Build();

            options.DefaultPolicy = defaultPolicy;

            options.AddPolicy("AdminPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("SystemAdmin", "Admin");
            });
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "SmartScheme";
            options.DefaultAuthenticateScheme = "SmartScheme";
            options.DefaultChallengeScheme = "SmartScheme";
        })
        .AddPolicyScheme("SmartScheme", "Smart Auth Selector", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                var hasBearer = context.Request.Headers["Authorization"].FirstOrDefault()?.StartsWith("Bearer ") == true;
                return hasBearer ? JwtBearerDefaults.AuthenticationScheme : CookieAuthenticationDefaults.AuthenticationScheme;
            };
        })

        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "Sky.Template";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(tokenConfig.AccessTokenExpireMinutes);
            options.SlidingExpiration = true;

            // ?? Redirectleri kapat
            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };

            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        })

        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = tokenConfig.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            // Burada localizer eriþimi için DI kullanacaðýz
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async ctx =>
                {
                    if (!ctx.Response.HasStarted)
                    {
                        var localizer = ctx.HttpContext.RequestServices
                            .GetRequiredService<IStringLocalizer<SharedResource>>();

                        ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        ctx.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = localizer["UnauthorizedAccess"]
                        });
                        await ctx.Response.WriteAsync(result);
                    }
                },
                OnForbidden = async ctx =>
                {
                    if (!ctx.Response.HasStarted)
                    {
                        var localizer = ctx.HttpContext.RequestServices
                            .GetRequiredService<IStringLocalizer<SharedResource>>();

                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = localizer["ForbiddenAccess"].Value
                        });
                        await ctx.Response.WriteAsync(result);
                    }
                }
            };
        });

        // Session
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }
}
