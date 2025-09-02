using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities;

public class DataCountEntity
{
    [DbManager.mColumn("count")] public int Count { get; set; }
}
