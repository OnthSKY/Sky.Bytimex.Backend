namespace Sky.Template.Backend.Contract.Responses.ErrorLogResponses;

public class ErrorLogResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public DateTime CreatedAt { get; set; }
}
