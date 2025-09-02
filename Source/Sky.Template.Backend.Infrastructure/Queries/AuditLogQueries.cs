namespace Sky.Template.Backend.Infrastructure.Queries;

internal static class AuditLogQueries
{
    internal const string Insert = @"INSERT INTO $db.acc_audit_log (activity_id, user_id, event_name, page_url, request_time, response_time, request_url, module_name, request_body, response_body, device, browser, application,device_family,device_type) 
                VALUES (@activity_id, @user_id, @event_name, @page_url, @request_time, @response_time, @request_url, @module_name, @request_body, @response_body, @device, @browser, @application,@device_family, @device_type); ";

    internal const string GetLogs = @"SELECT *, (usr.name + ' ' + usr.surname) as [user] FROM $db.acc_audit_log log INNER JOIN $db.users usr ON log.user_id = usr.id; ";

    internal const string GetLogsById = @"SELECT TOP 10 * FROM $db.acc_audit_log WHERE user_id = @user_id ORDER BY request_time DESC; ";
}
