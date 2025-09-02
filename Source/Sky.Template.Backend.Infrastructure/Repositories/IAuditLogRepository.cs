using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Infrastructure.Entities.AuditLog;
using Sky.Template.Backend.Infrastructure.Queries;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;
using Sky.Template.Backend.Contract.Requests.AuditLog;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IAuditLogRepository
{
    Task Execute(AuditLogParameters parameters, string applicationName);
    List<AuditLogEntity> GetLogs();
    List<AuditLogEntity> GetLogsByUserId(int userId);
}

public class AuditLogRepository : IAuditLogRepository
{

    public async Task Execute(AuditLogParameters parameters, string applicationName)
    {
        DbManager.ExecuteNonQuery(AuditLogQueries.Insert, new Dictionary<string, dynamic>
        {
            {"@activity_id", parameters.ActivityId},
            {"@user_id", parameters.UserId},
            {"@event_name", string.IsNullOrEmpty(parameters.EventName) ? "" : parameters.EventName},
            {"@page_url", string.IsNullOrEmpty(parameters.PageUrl) ? "" : parameters.PageUrl},
            {"@request_time", parameters.RequestTime},
            {"@response_time", parameters.ResponseTime},
            {"@request_url", parameters.RequestUrl},
            {"@module_name", parameters.ModuleName},
            {"@request_body", parameters.RequestBody},
            {"@response_body", parameters.ResponseBody},
            {"@device", parameters.Device},
            {"@browser", parameters.Browser},
            {"@application", applicationName},
            {"@device_family", parameters.DeviceFamily},
            {"@device_type", parameters.DeviceType},

        }, GlobalSchema.Name);
    }

    public List<AuditLogEntity> GetLogs()
    {
        return DbManager.Read<AuditLogEntity>(AuditLogQueries.GetLogs, null, GlobalSchema.Name).ToList();
    }

    public List<AuditLogEntity> GetLogsByUserId(int userId)
    {
        return DbManager.Read<AuditLogEntity>(AuditLogQueries.GetLogsById, new Dictionary<string, dynamic>
        {
            {"@user_id", userId},

        }, GlobalSchema.Name).ToList();
    }
}
