using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Responses.Dashboard.Vendor;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Core.Extensions;

namespace Sky.Template.Backend.Application.Services.Vendor;

public interface IVendorDashboardService
{
    [HasPermission(Permissions.Dashboard.Vendor.Read)]
    Task<BaseControllerResponse<VendorSalesSummaryResponse>> GetVendorSalesSummaryAsync();
    [HasPermission(Permissions.Dashboard.Vendor.Read)]
    Task<BaseControllerResponse<VendorTopProductsResponse>> GetTopSellingProductsAsync();
    [HasPermission(Permissions.Dashboard.Vendor.Read)]
    Task<BaseControllerResponse<VendorMonthlyStatsResponse>> GetMonthlyOrderChartDataAsync();
    [HasPermission(Permissions.Dashboard.Vendor.Read)]
    Task<BaseControllerResponse<VendorPendingOperationsResponse>> GetPendingOperationsAsync();
}

public class VendorDashboardService : IVendorDashboardService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VendorDashboardService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetVendorId() => _httpContextAccessor.HttpContext.GetUserId();

    [HasPermission(Permissions.Dashboard.Vendor.Read)]
    public Task<BaseControllerResponse<VendorSalesSummaryResponse>> GetVendorSalesSummaryAsync()
    {
        var vendorId = GetVendorId();
        var summary = new VendorSalesSummaryResponse
        {
            TotalSales = 10000m,
            OrderCount = 150,
            AverageOrderValue = 10000m / 150m
        };
        return Task.FromResult(ControllerResponseBuilder.Success(summary));
    }

    [HasPermission(Permissions.Dashboard.Vendor.Read)]
    public Task<BaseControllerResponse<VendorTopProductsResponse>> GetTopSellingProductsAsync()
    {
        var vendorId = GetVendorId();
        var products = new VendorTopProductsResponse
        {
            Products = Enumerable.Range(1, 5).Select(i => new TopProductDto
            {
                ProductId = Guid.NewGuid(),
                Name = $"Product {i}",
                QuantitySold = 10 * i
            }).ToList()
        };
        return Task.FromResult(ControllerResponseBuilder.Success(products));
    }

    [HasPermission(Permissions.Dashboard.Vendor.Read)]
    public Task<BaseControllerResponse<VendorMonthlyStatsResponse>> GetMonthlyOrderChartDataAsync()
    {
        var vendorId = GetVendorId();
        var now = DateTime.UtcNow;
        var stats = new VendorMonthlyStatsResponse
        {
            Stats = Enumerable.Range(0, 6).Select(i =>
                new MonthlyOrderStatsDto
                {
                    Month = now.AddMonths(-i).ToString("yyyy-MM"),
                    Revenue = 1000m * (6 - i),
                    OrderCount = 10 * (6 - i)
                }).Reverse().ToList()
        };
        return Task.FromResult(ControllerResponseBuilder.Success(stats));
    }

    [HasPermission(Permissions.Dashboard.Vendor.Read)]
    public Task<BaseControllerResponse<VendorPendingOperationsResponse>> GetPendingOperationsAsync()
    {
        var vendorId = GetVendorId();
        var operations = new VendorPendingOperationsResponse
        {
            PendingOrders = 3,
            PendingKycVerifications = 1,
            PendingReturnRequests = 2
        };
        return Task.FromResult(ControllerResponseBuilder.Success(operations));
    }
}
