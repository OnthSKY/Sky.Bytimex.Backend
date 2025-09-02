using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.ErrorLog;

[TableName("error_logs")]
public class ErrorLogEntity
{
    [DbManager.mColumn("id")]
    public Guid Id { get; set; }

    [DbManager.mColumn("message")]
    public string Message { get; set; } = string.Empty;

    [DbManager.mColumn("stack_trace")]
    public string? StackTrace { get; set; }

    [DbManager.mColumn("source")]
    public string? Source { get; set; }

    [DbManager.mColumn("path")]
    public string? Path { get; set; }

    [DbManager.mColumn("method")]
    public string? Method { get; set; }

    [DbManager.mColumn("created_at")]
    public DateTime CreatedAt { get; set; }
}
