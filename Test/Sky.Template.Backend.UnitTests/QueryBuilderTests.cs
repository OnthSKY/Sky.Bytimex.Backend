using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;
using FluentAssertions;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class QueryBuilderTests
{
    private class TestEntity
    {
        [DbManager.mColumn("id")] public int Id { get; set; }
        [DbManager.mColumn("name")] public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void BuildSql_Pagination_Postgres()
    {
        var qb = new QueryBuilder(new PostgreSqlDialect())
            .From<TestEntity>("e")
            .Select("e.id")
            .OrderBy("e.id")
            .Page(2, 10);
        var (sql, _) = qb.BuildSql();
        sql.Should().Contain("LIMIT 10 OFFSET 10");
    }

    [Fact]
    public void Like_Vs_Ilike()
    {
        var qb1 = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("e").Where("e.name", "like", "abc");
        var (sql1, _) = qb1.BuildSql();
        sql1.Should().Contain("LIKE");
        var qb2 = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("e").Where("e.name", "ilike", "abc");
        var (sql2, _) = qb2.BuildSql();
        sql2.Should().Contain("ILIKE");
    }

    [Fact]
    public void In_And_Between_Parametrization()
    {
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("e")
            .Where("e.id", "in", new[] {1,2,3})
            .And("e.id", "between", new[] {4,5});
        var (sql, prms) = qb.BuildSql();
        sql.Should().Contain("IN (@p0,@p1,@p2)");
        prms.Should().HaveCount(5);
    }

    [Fact]
    public void Expression_Translation_Works()
    {
        var from = 5;
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("e")
            .Where<TestEntity>(x => x.Id >= from && x.Name == "A");
        var (sql, _) = qb.BuildSql();
        sql.Should().Contain("e.\"id\" >= @p0 AND e.\"name\" = @p1");
    }

    [Fact]
    public void StripOrderBy_ForCount()
    {
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("e").OrderBy("e.id");
        var (sql, _) = qb.BuildSql();
        var dialect = new PostgreSqlDialect();
        var countSql = dialect.CountWrap(dialect.StripOrderBy(sql));
        countSql.Should().NotContain("ORDER BY");
    }

    [Fact]
    public void ParameterPrefix_FromDialect()
    {
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("e").Where("e.id", "eq", 1);
        var (_, prms) = qb.BuildSql();
        prms.Keys.First().Should().StartWith("@");
    }

    [Fact]
    public void Page_Guardrails()
    {
        var qb = new QueryBuilder(new PostgreSqlDialect()).From<TestEntity>("e");
        Action act = () => qb.Page(0, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private class JsonEntity
    {
        [DbManager.mColumn("items")] public List<int> Items { get; set; } = new();
    }

    [Fact]
    public void Json_Column_Mapping()
    {
        var dt = new DataTable();
        dt.Columns.Add("items", typeof(string));
        dt.Rows.Add("[1,2]");
        var getMap = typeof(DbManager).GetMethod("GetColumnMappings", BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeof(JsonEntity));
        var map = (Dictionary<string, PropertyInfo>)getMap.Invoke(null, null)!;
        var mapMethod = typeof(DbManager).GetMethod("MapResultToModelList", BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(typeof(JsonEntity));
        var list = (List<JsonEntity>)mapMethod.Invoke(null, new object[] { dt, map })!;
        list.Single().Items.Should().BeEquivalentTo(new[] {1,2});
    }
}
