
namespace Sky.Template.Backend.Contract.Requests.AuditLog;

public class AuditLogParameters
{
    public Guid ActivityId { get; set; }

    public int UserId { get; set; }

    public string EventName { get; set; }

    public string PageUrl { get; set; }

    public DateTime RequestTime { get; set; }

    public DateTime ResponseTime { get; set; }

    public string RequestUrl { get; set; }

    public string ModuleName { get; set; }

    public string RequestBody { get; set; }

    public string ResponseBody { get; set; }

    public string User { get; set; }

    public string Device { get; set; }

    public string Browser { get; set; }

    public string Application { get; set; }
    public string DeviceFamily { get; set; }
    public string DeviceType { get; set; }

}
