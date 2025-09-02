using Sky.Template.Backend.Core.Constants;
using System.Net;
using Sky.Template.Backend.Contract.Requests.ErrorLogs;
using Sky.Template.Backend.Contract.Responses.ErrorLogResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Infrastructure.Entities.ErrorLog;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminErrorLogService
{
    Task LogErrorAsync(CreateErrorLogRequest request);
    Task<BaseControllerResponse<ErrorLogListPaginatedResponse>> GetAllAsync(GridRequest request);
    Task<BaseControllerResponse<ErrorLogResponse>> GetByIdAsync(Guid id);
}

public class AdminErrorLogService : IAdminErrorLogService
{
    private readonly IErrorLogRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminErrorLogService(IErrorLogRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    [ValidationAspect(typeof(Validators.FluentValidation.ErrorLog.CreateErrorLogRequestValidator))]
    public async Task LogErrorAsync(CreateErrorLogRequest request)
    {
        var entity = new ErrorLogEntity
        {
            Id = Guid.NewGuid(),
            Message = request.Message,
            StackTrace = request.StackTrace,
            Source = request.Source,
            Path = request.Path,
            Method = request.Method,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.InsertAsync(entity);
     }

    [HasPermission(Permissions.ErrorLogs.View)]
    public async Task<BaseControllerResponse<ErrorLogListPaginatedResponse>> GetAllAsync(GridRequest request)
    {
        var (logs, totalCount) = await _repository.GetAllAsync(request);
        var response = new ErrorLogListPaginatedResponse
        {
            Logs = new Core.BaseResponse.PaginatedData<ErrorLogResponse>
            {
                Items = logs.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        };
        return ControllerResponseBuilder.Success(response);
    }

    [HasPermission(Permissions.ErrorLogs.View)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<ErrorLogResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            return ControllerResponseBuilder.Failure<ErrorLogResponse>("ErrorLogNotFound",  HttpStatusCode.NotFound);
        var dto = MapToResponse(entity);
        return ControllerResponseBuilder.Success(dto);
    }

    private static ErrorLogResponse MapToResponse(ErrorLogEntity entity)
    {
        return new ErrorLogResponse
        {
            Id = entity.Id,
            Message = entity.Message,
            StackTrace = entity.StackTrace,
            Source = entity.Source,
            Path = entity.Path,
            Method = entity.Method,
            CreatedAt = entity.CreatedAt
        };
    }
}
