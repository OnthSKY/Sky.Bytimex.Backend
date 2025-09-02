using Newtonsoft.Json;

namespace Sky.Template.Backend.Core.Exceptions;

public class ErrorDetails
{
    public string Message { get; set; }
    public string TransactionId { get; set; }
    public int StatusCode { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

