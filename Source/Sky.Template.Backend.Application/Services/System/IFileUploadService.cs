using Sky.Template.Backend.Core.Constants;
using System.Net;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Application.Validators.FluentValidation.FileUpload;
using Sky.Template.Backend.Contract.Requests.FileUploads;
using Sky.Template.Backend.Contract.Responses.FileUploadResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories.System;

namespace Sky.Template.Backend.Application.Services.System;

public interface IFileUploadService
{
    Task<BaseControllerResponse<List<FileUploadResponse>>> GetAllAsync();
    Task<BaseControllerResponse<FileUploadResponse>> GetByIdAsync(Guid id);
    Task<BaseControllerResponse<FileUploadResponse>> CreateAsync(CreateFileUploadRequest request);
    Task<BaseControllerResponse<FileUploadResponse>> UpdateAsync(Guid id, UpdateFileUploadRequest request);
    Task<BaseControllerResponse> DeleteAsync(Guid id);
}

public class FileUploadService : IFileUploadService
{
    private readonly IFileUploadRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FileUploadService(IFileUploadRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    [HasPermission(Permissions.FileUploads.View)]
    public async Task<BaseControllerResponse<List<FileUploadResponse>>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        var list = entities.Select(MapToResponse).ToList();
        return ControllerResponseBuilder.Success(list);
    }

    [HasPermission(Permissions.FileUploads.View)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<FileUploadResponse>> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            throw new NotFoundException("FileUploadNotFound", id.ToString());
        var dto = MapToResponse(entity);
        return ControllerResponseBuilder.Success(dto);
    }

    [ValidationAspect(typeof(CreateFileUploadRequestValidator))]
    [HasPermission(Permissions.FileUploads.Create)]
    public async Task<BaseControllerResponse<FileUploadResponse>> CreateAsync(CreateFileUploadRequest request)
    {
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var entity = new FileUploadEntity
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            FileExtension = request.FileExtension,
            FileSize = request.FileSize,
            FileUrl = request.FileUrl,
            FileType = request.FileType,
            Context = request.Context,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        var created = await _repository.CreateAsync(entity);
        var dto = MapToResponse(created);
        return ControllerResponseBuilder.Success(dto, "FileUploadCreated",  HttpStatusCode.Created);
    }

    [ValidationAspect(typeof(UpdateFileUploadRequestValidator))]
    [HasPermission(Permissions.FileUploads.Update)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<FileUploadResponse>> UpdateAsync(Guid id, UpdateFileUploadRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
            throw new NotFoundException("FileUploadNotFound", id.ToString());
        var userId = _httpContextAccessor.HttpContext.GetUserId();
        entity.FileName = request.FileName;
        entity.FileExtension = request.FileExtension;
        entity.FileSize = request.FileSize;
        entity.FileUrl = request.FileUrl;
        entity.FileType = request.FileType;
        entity.Context = request.Context;
        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        var updated = await _repository.UpdateAsync(entity);
        var dto = MapToResponse(updated);
        return ControllerResponseBuilder.Success(dto, "FileUploadUpdated");
    }

    [HasPermission(Permissions.FileUploads.Delete)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteAsync(Guid id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
            throw new NotFoundException("FileUploadNotFound", id.ToString());
        return ControllerResponseBuilder.Success();
    }

    private static FileUploadResponse MapToResponse(FileUploadEntity entity)
    {
        return new FileUploadResponse
        {
            Id = entity.Id,
            FileName = entity.FileName,
            FileExtension = entity.FileExtension,
            FileSize = entity.FileSize,
            FileUrl = entity.FileUrl,
            FileType = entity.FileType,
            Context = entity.Context,
            UploadedBy = entity.UploadedBy,
            UploadedAt = entity.UploadedAt,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = entity.UpdatedBy
        };
    }
}

