using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.Template.Backend.Application.Services.System;
using Sky.Template.Backend.Contract.Requests.FileUploads;
using Sky.Template.Backend.WebAPI.Controllers.Base;

namespace Sky.Template.Backend.WebAPI.Controllers.System;

[ApiController]
[Route("api/file-uploads")]
[ApiVersion("1.0")]
[Authorize]
public class FileUploadController : CustomBaseController
{
    private readonly IFileUploadService _fileUploadService;

    public FileUploadController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpGet("v{version:apiVersion}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetFileUploads()
    {
        return await HandleServiceResponseAsync(() => _fileUploadService.GetAllAsync());
    }

    [HttpGet("v{version:apiVersion}/{id}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetFileUpload(Guid id)
    {
        return await HandleServiceResponseAsync(() => _fileUploadService.GetByIdAsync(id));
    }

    [HttpPost("v{version:apiVersion}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> CreateFileUpload([FromBody] CreateFileUploadRequest request)
    {
        return await HandleServiceResponseAsync(() => _fileUploadService.CreateAsync(request));
    }

    [HttpPut("v{version:apiVersion}/{id}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> UpdateFileUpload(Guid id, [FromBody] UpdateFileUploadRequest request)
    {
        return await HandleServiceResponseAsync(() => _fileUploadService.UpdateAsync(id, request));
    }

    [HttpDelete("v{version:apiVersion}/{id}")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> DeleteFileUpload(Guid id)
    {
        return await HandleServiceResponseAsync(() => _fileUploadService.DeleteAsync(id));
    }
}

