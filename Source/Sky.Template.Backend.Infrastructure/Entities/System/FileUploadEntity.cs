using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Entities.Base;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

namespace Sky.Template.Backend.Infrastructure.Entities.System;

[TableName("file_uploads")]
public class FileUploadEntity : BaseEntity<Guid>
{
    [DbManager.mColumn("file_name")]
    public string FileName { get; set; } = string.Empty;

    [DbManager.mColumn("file_extension")]
    public string FileExtension { get; set; } = string.Empty;

    [DbManager.mColumn("file_size")]
    public int FileSize { get; set; }

    [DbManager.mColumn("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [DbManager.mColumn("file_type")]
    public string FileType { get; set; } = string.Empty;

    [DbManager.mColumn("context")]
    public string? Context { get; set; }

    [DbManager.mColumn("uploaded_by")]
    public Guid UploadedBy { get; set; }

    [DbManager.mColumn("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    [DbManager.mColumn("status")]
    public string Status { get; set; } = string.Empty;
}

