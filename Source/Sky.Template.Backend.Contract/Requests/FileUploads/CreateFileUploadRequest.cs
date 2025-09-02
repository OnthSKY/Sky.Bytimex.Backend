namespace Sky.Template.Backend.Contract.Requests.FileUploads;

public class CreateFileUploadRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public int FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string Status { get; set; } = "ACTIVE";
}

