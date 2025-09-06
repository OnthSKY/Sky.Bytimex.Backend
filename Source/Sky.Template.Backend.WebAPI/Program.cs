using Autofac;
using Autofac.Extensions.DependencyInjection;
using Sky.Template.Backend.Application.DependencyResolvers.Autofac;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Middleware;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Initializer;
using Sky.Template.Backend.Infrastructure.Localization;
using Sky.Template.Backend.WebAPI.Configurations;
using Sky.Template.Backend.WebAPI.Filters;
using Sky.Template.Backend.WebAPI.Middleware;
using Swashbuckle.AspNetCore.SwaggerUI;
using Sky.Template.Backend.Core.Utilities;
using StackExchange.Redis;
using Sky.Template.Backend.Application.Mappings;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var config = Utils.GetConfiguration();
var allowedOrigins = new[] {
    "http://localhost:3000"

};

builder.Services.AddMemoryCache();

builder.Services.AddControllers(config =>
{
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;

}).AddXmlDataContractSerializerFormatters().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new NullableDateTimeConverter());
});

builder.Services.AddAutoMapper(typeof(ReferralRewardMappingProfile));

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader()
    );

});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // v1, v2
    options.SubstituteApiVersionInUrl = true;
});

  builder.Services.AddHttpContextAccessor();
  builder.Services.AddScoped<ILanguageResolver, WebLanguageResolver>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SwaggerFileOperationFilter>();

});
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.Configure<EncryptionConfig>(config.GetSection("Encryption"));

builder.Services.AddAuthorizationAndAuthentication(config);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireRole("ADMIN", "DEVELOPER");
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
builder.Services.AddLocalization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddTokenBucketLimiter("token_bucket_policy", opt =>
    {
        opt.TokenLimit = 100;
        opt.TokensPerPeriod = 1;
        opt.ReplenishmentPeriod = TimeSpan.FromMilliseconds(500);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
        opt.AutoReplenishment = true;
    });

    options.AddSlidingWindowLimiter("sliding_window_sensitive", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 3;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiter");
        logger.LogWarning("Rate limit exceeded for {Path}", context.HttpContext.Request.Path);

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too Many Requests", token);
    };
});

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new AutofacApplicationModule());
    containerBuilder.RegisterModule(new AutofacInfrastructureModule());
});

builder.Services.Configure<AzureOptConfig>(config.GetSection("AzureOptConfig"));
builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var options = provider.GetRequiredService<IOptions<AzureOptConfig>>();
    return new BlobServiceClient(options.Value.ConnectionString);
});

builder.Services.Configure<AzureAdLoginConfig>(config.GetSection("AzureAdLoginConfig"));

builder.Services.Configure<CacheConfig>(config.GetSection("Cache"));


var cacheOptions = config.GetSection("Cache").Get<CacheConfig>() ?? new CacheConfig();
if (cacheOptions.Provider.ToLower() == "redis")
{
    builder.Services.Configure<RedisConfig>(config.GetSection("Redis"));
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var redisConfig = sp.GetRequiredService<IOptions<RedisConfig>>().Value;
        return ConnectionMultiplexer.Connect(redisConfig.ConnectionString);
    });
}


var app = builder.Build();
ServiceTool.Create(app.Services);
app.UseMiddleware<CookieToHeaderMiddleware>();
app.UseMiddleware<SilentAuthMiddleware>(); // <--- BURAYA EKLE


using (var scope = app.Services.CreateScope())
{
    LocalizationProvider.Localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>();
}
var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("tr") };


app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    DatabaseInitializer.InitializeDatabase(builder.Configuration);
    DataInitializer.SeedData(builder.Configuration);
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocExpansion(DocExpansion.List);

        options.ConfigObject.AdditionalItems["withCredentials"] = true;
        foreach (var description in apiVersionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Sky Template API {description.GroupName.ToUpperInvariant()}"
            );
        }
    });
}
//app.Use(async (context, next) =>
//{
//    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
//    context.Response.Headers["Pragma"] = "no-cache";
//    context.Response.Headers["Expires"] = "0";
//    var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
//    if (context.Request.Method == "POST" &&
//        !context.Request.Path.StartsWithSegments("/api"))
//    {
//        await antiforgery.ValidateRequestAsync(context);
//    }

//    await next();
//});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ImpersonationMiddleware>();
//app.UseMiddleware<ClaimsUpdateMiddleware>();
app.UseMiddleware<SchemaMiddleware>();
app.UseMiddleware<AuditLogMiddleware>();
app.ConfigureCustomExceptionMiddleware();

app.MapControllers().RequireRateLimiting("token_bucket_policy");

app.MapPost("/api/auth/login", () => Results.Ok())
   .RequireRateLimiting("sliding_window_sensitive");

app.MapPost("/api/auth/register", () => Results.Ok())
   .RequireRateLimiting("sliding_window_sensitive");

app.MapPost("/api/payments/checkout", () => Results.Ok())
   .RequireRateLimiting("sliding_window_sensitive");

app.Run();
