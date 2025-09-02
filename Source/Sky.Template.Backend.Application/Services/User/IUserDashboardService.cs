using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Responses.Dashboard.User;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Extensions;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserDashboardService
{
    [HasPermission(Permissions.Dashboard.User.Read)]
    Task<BaseControllerResponse<UserPurchaseSummaryResponse>> GetUserPurchaseSummaryAsync();
    [HasPermission(Permissions.Dashboard.User.Read)]
    Task<BaseControllerResponse<UserRecentProductsResponse>> GetRecentlyViewedProductsAsync();
    [HasPermission(Permissions.Dashboard.User.Read)]
    Task<BaseControllerResponse<UserRecommendedProductsResponse>> GetRecommendedProductsAsync();
}

public class UserDashboardService : IUserDashboardService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserDashboardService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetUserId() => _httpContextAccessor.HttpContext.GetUserId();

    [HasPermission(Permissions.Dashboard.User.Read)]
    public Task<BaseControllerResponse<UserPurchaseSummaryResponse>> GetUserPurchaseSummaryAsync()
    {
        var userId = GetUserId();
        var summary = new UserPurchaseSummaryResponse
        {
            TotalPurchases = 25,
            TotalSpent = 2500m,
            UsedCoupons = 5
        };
        return Task.FromResult(ControllerResponseBuilder.Success(summary));
    }

    [HasPermission(Permissions.Dashboard.User.Read)]
    public Task<BaseControllerResponse<UserRecentProductsResponse>> GetRecentlyViewedProductsAsync()
    {
        var userId = GetUserId();
        var products = new UserRecentProductsResponse
        {
            Products = Enumerable.Range(1, 5).Select(i => new ProductViewDto
            {
                ProductId = Guid.NewGuid(),
                Name = $"Product {i}"
            }).ToList()
        };
        return Task.FromResult(ControllerResponseBuilder.Success(products));
    }

    [HasPermission(Permissions.Dashboard.User.Read)]
    public Task<BaseControllerResponse<UserRecommendedProductsResponse>> GetRecommendedProductsAsync()
    {
        var userId = GetUserId();
        var products = new UserRecommendedProductsResponse
        {
            Products = Enumerable.Range(6, 5).Select(i => new ProductViewDto
            {
                ProductId = Guid.NewGuid(),
                Name = $"Product {i}"
            }).ToList()
        };
        return Task.FromResult(ControllerResponseBuilder.Success(products));
    }
}
