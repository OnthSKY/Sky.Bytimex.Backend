namespace Sky.Template.Backend.Core.Exceptions;

public class DatabaseException : Exception
{
    public int ErrorCode { get; }
    public string? ProcedureName { get; }
    public string? Query { get; }
    public object? Parameters { get; }
    public string? CallerMethod { get; }
    public string MessageKey { get; }

    public DatabaseException(
        string messageKey,
        Exception? innerException = null,
        int errorCode = 0,
        string? procedureName = null,
        string? query = null,
        object? parameters = null,
        string? callerMethod = null
    ) : base(messageKey, innerException)
    {
        MessageKey = messageKey;
        ErrorCode = errorCode;
        ProcedureName = procedureName;
        Query = query;
        Parameters = parameters;
        CallerMethod = callerMethod;
    }
}
