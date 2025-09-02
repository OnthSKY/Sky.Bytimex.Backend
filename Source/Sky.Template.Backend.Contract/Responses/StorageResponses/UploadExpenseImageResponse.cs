namespace Sky.Template.Backend.Contract.Responses.StorageResponses;


public class UploadExpenseImageResponse  
{
    public List<string> BlobStorageFilePath { get; set; } = [];
    public List<Dictionary<string, string>> Metadata { get; set; } = [];
}
