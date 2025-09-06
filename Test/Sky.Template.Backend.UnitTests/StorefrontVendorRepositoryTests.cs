using FluentAssertions;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class StorefrontVendorRepositoryTests
{
    [Fact]
    public void BuildListSql_DefaultsAndSupportsFeatures()
    {
        var repo = new StorefrontVendorRepository();
        var request = new GridRequest
        {
            Page = 2,
            PageSize = 5,
            SearchValue = "acme",
            OrderColumn = "product_count",
            OrderDirection = "ASC"
        };
        var (sql, prms) = repo.BuildListSql(request);
        sql.Should().Contain("v.status = @status");
        prms.Should().ContainKey("@status").WhoseValue.Should().Be("ACTIVE");
        sql.Should().Contain("ORDER BY product_count ASC");
        prms.Should().ContainKey("@Offset").WhoseValue.Should().Be(5);
        prms.Should().ContainKey("@Search_v_name").WhoseValue.Should().Be("%acme%");
    }
}
