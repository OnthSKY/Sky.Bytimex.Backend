using FluentAssertions;
using Sky.Template.Backend.Core.Localization;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class TranslationSqlBuilderTests
{
    [Fact]
    public void Build_GeneratesJoins_AndProjection()
    {
        var cfg = new TranslationConfig
        {
            TranslationTable = "sys.product_translations",
            ForeignKeyColumn = "product_id",
            LanguageColumn = "language_code",
            MainAlias = "p",
            ProjectedColumns = new[]
            {
                new TranslationColumn("name"),
                new TranslationColumn("description"),
                new TranslationColumn("key")
            }
        };

        var (joins, proj) = TranslationSqlBuilder.Build(cfg);
        joins.Should().Contain("LEFT JOIN LATERAL");
        proj.Should().Contain("COALESCE(pt_lang.name, pt_any.name) AS name");
        proj.Should().Contain("COALESCE(pt_lang.description, pt_any.description) AS description");
        proj.Should().Contain("COALESCE(pt_lang.key, pt_any.key) AS key");
    }

    [Fact]
    public void Build_NoColumns_ReturnsEmpty()
    {
        var cfg = new TranslationConfig
        {
            TranslationTable = "sys.x_trans",
            ForeignKeyColumn = "x_id"
        };

        var (joins, proj) = TranslationSqlBuilder.Build(cfg);
        joins.Should().BeEmpty();
        proj.Should().BeEmpty();
    }
}
