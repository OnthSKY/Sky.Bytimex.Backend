using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.Auth;

public class SchemaNameEntity
{
    [DbManager.mColumn("schema_name")]
    public string SchemaName { get; set; }
}
