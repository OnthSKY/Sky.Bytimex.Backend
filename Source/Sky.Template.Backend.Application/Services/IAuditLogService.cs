using Sky.Template.Backend.Contract.Requests.AuditLog;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services;

public interface IAuditLogService
{
    Task Execute(AuditLogParameters auditLogRecord);
}

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task Execute(AuditLogParameters auditLogRecord)
    {
        _auditLogRepository.Execute(auditLogRecord, "Backend"); // Değişecek
    }

}
