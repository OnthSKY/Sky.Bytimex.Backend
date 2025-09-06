using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class ProductRepositoryVendorFilterTests
{
    [Fact]
    public void BuildLocalizedProductsSql_WithVendorSlug_JoinsVendor()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        accessor.HttpContext!.Request.Headers["Accept-Language"] = "en";
        var repo = new ProductRepository(accessor);
        var request = new ProductFilterRequest
        {
            Page = 1,
            PageSize = 10,
            Filters = new Dictionary<string, string> { { "vendor_slug", "acme" } }
        };
        var (sql, prms) = repo.BuildLocalizedProductsSql(request, true);
        sql.Should().Contain("JOIN sys.vendors v ON v.id = p.vendor_id");
        sql.Should().Contain("LOWER(v.slug) = @vendor_slug");
        prms.Should().ContainKey("@vendor_slug").WhoseValue.Should().Be("acme");
    }
}
