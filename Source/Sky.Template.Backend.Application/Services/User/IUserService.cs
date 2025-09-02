using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Models;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Contract.Requests.Users;
using Sky.Template.Backend.Contract.Responses.UserResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Infrastructure.Repositories;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserService
{
    Task<bool> UpdateUserImageFromAzureLoginAsync(string imagePath, string userId, string schemaName);
    Task<string?> UploadUserImageToAzureBlobStorageAsync(byte[] userImage, string userId, string userName);

    Task<BaseControllerResponse<SingleUserResponse>> GetUserDtoByIdAsync(Guid userId);
    Task<BaseControllerResponse<SingleUserResponse>> GetUserDtoByIdOrThrowAsync(Guid userId);
    Task<BaseControllerResponse<SingleUserResponse>> GetUserDtoByEmailAsync(string email);
    Task<BaseControllerResponse<SingleUserResponse>> GetUserDtoByEmailOrThrowAsync(string email);

    Task<BaseControllerResponse<SingleUserResponse>> UpdateUserAsync(UpdateUserRequest request);
}

public class UserService : IUserService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IUserRepository userRepository, BlobServiceClient blobServiceClient, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _blobServiceClient = blobServiceClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> UpdateUserImageFromAzureLoginAsync(string imagePath, string userId, string schemaName)
        => await _userRepository.UpdateUserImageFromAzureLoginAsync(imagePath, userId, schemaName);

    public async Task<string?> UploadUserImageToAzureBlobStorageAsync(byte[] userImage, string userId, string userName)
    {
        try
        {
            var extension = ".jpg";
            var safeUserName = Utils.Slugify(userName);
            var fileName = $"{GlobalSchema.Name}-{userId}-{safeUserName}{extension}";

            var containerClient = _blobServiceClient.GetBlobContainerClient("sanflow-user-profile-images");
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(userImage);
            await blobClient.UploadAsync(stream, overwrite: true);

            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = "image/jpeg" });

            var metadata = new Dictionary<string, string>
            {
                { "userId", userId },
                { "schemaName", GlobalSchema.Name }
            };
            await blobClient.SetMetadataAsync(metadata);

            return blobClient.Uri.ToString();
        }
        catch
        {
            return null;
        }
    }

    [EnsureUserIsValid(new[] { "userId" })]
    public async Task<BaseControllerResponse<SingleUserResponse>> GetUserDtoByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetUserWithRoleByIdAsync(userId);

        return ControllerResponseBuilder.Success(new SingleUserResponse
        {
            User = new UserWithRoleDto
            {
                Id = user.Id,
                FirstName = user.Name,
                LastName = user.Surname,
                Email = user.Email,
                UserImagePath = user.ImagePath,
                Role = new Role
                {
                    Id = user.RoleId,
                    Name = user.RoleName,
                    Description = user.RoleDescription
                }
            }
        });
    }

    [EnsureUserIsValid(new[] { "userId" })]
    public async Task<BaseControllerResponse<SingleUserResponse>> GetUserDtoByIdOrThrowAsync(Guid userId)
    {
        var user = await _userRepository.GetUserWithRoleByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFoundWithId", userId);

        return ControllerResponseBuilder.Success(new SingleUserResponse
        {
            User = new UserWithRoleDto
            {
                Id = user.Id,
                FirstName = user.Name,
                LastName = user.Surname,
                Email = user.Email,
                UserImagePath = user.ImagePath,
                CustomFieldList = user.CustomFields,
                Role = new Role
                {
                    Id = user.RoleId,
                    Name = user.RoleName,
                    Description = user.RoleDescription,
                    PermissionNamesRaw = user.PermissionNamesRaw
                }
            }
        });
    }

    public async Task<BaseControllerResponse<SingleUserResponse>> GetUserDtoByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserWithRoleByEmailAsync(email);

        return ControllerResponseBuilder.Success(new SingleUserResponse
        {
            User = new UserWithRoleDto
            {
                Id = user.Id,
                FirstName = user.Name,
                LastName = user.Surname,
                Email = user.Email,
                UserImagePath = user.ImagePath,
                Role = new Role
                {
                    Id = user.RoleId,
                    Name = user.RoleName,
                    Description = user.RoleDescription,
                    PermissionNamesRaw = user.PermissionNamesRaw
                }
            }
        });
    }

    public async Task<BaseControllerResponse<SingleUserResponse>> GetUserDtoByEmailOrThrowAsync(string email)
    {
        var response = await GetUserDtoByEmailAsync(email);
        if (response?.Data?.User is null)
            throw new NotFoundException("UserNotFoundWithEmail", email);
        return response;
    }

    public async Task<BaseControllerResponse<SingleUserResponse>> UpdateUserAsync(UpdateUserRequest request)
    {
        var existing = await _userRepository.GetUserWithRoleByIdAsync(request.Id);
        if (existing == null)
            throw new NotFoundException("UserNotFoundWithId", request.Id);

        var entity = new UserEntity
        {
            Id = request.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Status = request.Status,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = _httpContextAccessor.HttpContext.GetUserId()
        };

        var updated = await _userRepository.UpdateUserAsync(entity);

        return ControllerResponseBuilder.Success(new SingleUserResponse
        {
            User = new UserWithRoleDto
            {
                Id = updated.Id,
                FirstName = updated.FirstName,
                LastName = updated.LastName,
                Email = updated.Email,
                UserImagePath = updated.ImagePath,
                Role = new Role
                {
                    Id = existing.RoleId,
                    Name = existing.RoleName,
                    Description = existing.RoleDescription
                }
            }
        });
    }
}
