using System.Collections.Generic;
using FluentAssertions;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class QueryBuilderTests
{
    private class TestEntity { }

    [Fact]
    public void WithSearch_BuildsGroupedLikes()
    {
        var qb = new QueryBuilder(new PostgreSqlDialect())
            .From<TestEntity>("p")
            .WithSearch("abc", new[] { "p.name", "p.sku" });
        var (sql, prms) = qb.Build();
        sql.Should().Contain("(p.name LIKE @p0 OR p.sku LIKE @p0)");
        prms.Should().ContainKey("@p0").WhoseValue.Should().Be("%abc%");
    }

    [Fact]
    public void WithFilters_Eq_Like_And_Raw()
    {
        var filters = new Dictionary<string,string>
        {
            ["slug"] = "x",
            ["name"] = "phone",
            ["from"] = "2025-01-01"
        };
        var mappings = new Dictionary<string,string>
        {
            ["slug"] = "p.slug",
            ["name"] = "p.name",
            ["from"] = "p.created_at >= @MinDate"
        };
        var likeKeys = new HashSet<string>{"name"};
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("p")
            .WithFilters(filters, mappings, likeKeys);
        var (sql, prms) = qb.Build();
        sql.Should().Contain("p.slug = @p0");
        sql.Should().Contain("p.name LIKE @p1");
        sql.Should().Contain("p.created_at >= @MinDate");
        prms["@p0"].Should().Be("x");
        prms["@p1"].Should().Be("%phone%");
        prms["@MinDate"].Should().Be("2025-01-01");
    }

    [Fact]
    public void OrderByMapped_Fallback()
    {
        var mappings = new Dictionary<string,string> { ["name"] = "p.name" };
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("p")
            .OrderByMapped("hacker", "ASC", mappings, "p.created_at DESC");
        var (sql, _) = qb.Build();
        sql.Trim().Should().EndWith("ORDER BY p.created_at DESC");
    }

    [Fact]
    public void Pagination_Clause_And_Params()
    {
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("p")
            .Paginate(3, 20);
        var (sql, prms) = qb.Build();
        sql.Should().EndWith("LIMIT @PageSize OFFSET @Offset");
        prms["@Offset"].Should().Be(40);
        prms["@PageSize"].Should().Be(20);
    }

    [Fact]
    public void ToCountSql_StripsTopLevelOrder()
    {
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("p")
            .OrderBy("p.created_at DESC")
            .Paginate(1,10);
        var countSql = qb.ToCountSql();
        countSql.Should().NotContain("ORDER BY p.created_at DESC");
    }
}
