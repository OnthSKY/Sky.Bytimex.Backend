using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using System.Net;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.Auth;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Sky.Template.Backend.Infrastructure.Entities.Auth;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Models;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Application.Validators.FluentValidation.Auth;
using Sky.Template.Backend.Core.Localization;

namespace Sky.Template.Backend.Application.Services;

public interface ISharedAuthService
{
    Task<BaseControllerResponse<AuthResponse>> AuthenticateWithPasswordAsync(AuthRequest request, string ip = "");
    Task<BaseControllerResponse<AuthResponse>> AuthenticateByEmailAsync(AuthWithoutPasswordRequest request);
    Task<BaseControllerResponse<AuthResponse>> RefreshTokenAsync(string refreshToken);
    Task<BaseControllerResponse> LogoutAsync(string refreshToken, string? clientIp);
    Task<BaseControllerResponse<AuthResponse>> RegisterUserAsync(RegisterUserRequest request);
    Task<BaseControllerResponse<AuthResponse>> RegisterVendorAsync(RegisterVendorRequest request);
}

public class SharedAuthService : ISharedAuthService
{
    protected readonly IAuthRepository _authRepository;
    protected readonly IUserService _userService;
    protected readonly IPasswordHashService _passwordService;
    protected readonly ITokenService _tokenService;
    protected readonly TokenManagerConfig _tokenManagerConfig;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IReferralRewardRepository _referralRewardRepository;

    public SharedAuthService(
        IAuthRepository authRepository,
        IOptions<TokenManagerConfig> tokenOptions,
        IUnitOfWork unitOfWork,
        IPasswordHashService passwordService,
        ITokenService tokenService,
        IUserService userService,
        IReferralRewardRepository referralRewardRepository)
    {
        _authRepository = authRepository;
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _tokenManagerConfig = tokenOptions.Value;
        _userService = userService;
        _referralRewardRepository = referralRewardRepository;
    }

    public async Task<BaseControllerResponse> LogoutAsync(string refreshToken, string? clientIp)
    {
        var isRevoked = await _authRepository.RevokeRefreshTokenAsync(refreshToken, clientIp);

        if (!isRevoked)
        {
            throw new UnAuthorizedException(SharedResourceKeys.RefreshTokenNotFound);
        }

        return ControllerResponseBuilder.Success(SharedResourceKeys.LogoutSuccessful, HttpStatusCode.OK);
    }

    [ValidationAspect(typeof(RegisterUserRequestValidator))]
    public async Task<BaseControllerResponse<AuthResponse>> RegisterUserAsync(RegisterUserRequest request)
    {
        Guid? referrerId = null;

        if (!string.IsNullOrWhiteSpace(request.ReferralCode))
        {
            if (!Guid.TryParse(request.ReferralCode, out var refGuid))
            {
                throw new BusinessRulesException(SharedResourceKeys.ReferralsCodeInvalid);
            }

            var refUserResponse = await _userService.GetUserDtoByIdOrThrowAsync(refGuid);

            if (refUserResponse?.Data?.User is null)
            {
                throw new BusinessRulesException(SharedResourceKeys.ReferralsCodeNotFound);
            }

            if (refUserResponse.Data.User.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRulesException(SharedResourceKeys.ReferralsSelfReferralNotAllowed);
            }

            referrerId = refUserResponse.Data.User.Id;
        }

        var registerRequest = new RegisterRequest
        {
            Email = request.Email,
            Phone = request.Phone,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        return await RegisterWithRoleAsync(registerRequest, DefaultRoles.USER, request.PreferredLanguage, referrerId);
    }

    [ValidationAspect(typeof(RegisterVendorRequestValidator))]
    public Task<BaseControllerResponse<AuthResponse>> RegisterVendorAsync(RegisterVendorRequest request)
        => RegisterWithRoleAsync(request, DefaultRoles.VENDOR);

    [ValidationAspect(typeof(AuthRequestValidator))]
    public async Task<BaseControllerResponse<AuthResponse>> AuthenticateWithPasswordAsync(AuthRequest request, string ip)
    {
        var usernameType = UsernameTypeValidation(request.Username);
        var accountUser = usernameType switch
        {
            UsernameLoginType.Email => await _authRepository.GetSystemUserByEmailAsync(request.Username),
            UsernameLoginType.Phone => await _authRepository.GetSystemUserByPhoneByAsync(request.Username),
            _ => null
        };

        if (accountUser is null)
        {
            throw new BusinessRulesException(SharedResourceKeys.UserNotFound);
        }

        if (!_passwordService.VerifyPassword(request.Password, accountUser.Password))
        {
            throw new BusinessRulesException(SharedResourceKeys.InvalidCredentials);
        }

        var response = await CreateAuthResponseAsync(accountUser, ip);
        return ControllerResponseBuilder.Success(response, SharedResourceKeys.LoginSuccessful);
    }

    [ValidationAspect(typeof(AuthWithoutPasswordRequestValidator))]
    public async Task<BaseControllerResponse<AuthResponse>> AuthenticateByEmailAsync(AuthWithoutPasswordRequest request)
    {
        var accountUser = await _authRepository.GetSystemUserByEmailAsync(request.Username);
        if (accountUser is null)
        {
            throw new NotFoundException(SharedResourceKeys.UserNotFoundWithEmails, request.Username);
        }
        var response = await CreateAuthResponseAsync(accountUser);
        return ControllerResponseBuilder.Success(response, SharedResourceKeys.LoginSuccessful);
    }

    public async Task<BaseControllerResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        var refreshEntity = await _authRepository.GetRefreshTokenAsync(refreshToken);

        if (refreshEntity == null || refreshEntity.ExpirationDate < DateTime.UtcNow)
        {
            throw new UnAuthorizedException(SharedResourceKeys.RefreshTokenInvalid);
        }

        var accountUser = await _authRepository.GetSystemUserByIdAndSchemaAsync(refreshEntity.UserId);
        if (accountUser is null)
        {
            throw new UnAuthorizedException(SharedResourceKeys.UserNotFound);
        }

        var authResponse = await CreateAuthResponseAsync(accountUser);

        return ControllerResponseBuilder.Success(authResponse, SharedResourceKeys.RefreshSuccessful);
    }

    protected async Task<BaseControllerResponse<AuthResponse>> RegisterWithRoleAsync(RegisterRequest request, DefaultRoles role, string? preferredLanguage = null, Guid? referredBy = null)
    {
        var existingUser = await _authRepository.GetSystemUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new BusinessRulesException(SharedResourceKeys.UsersEmailAlreadyExists);
        }

        var hashedPassword = _passwordService.HashPassword(request.Password);

        var userId = Guid.NewGuid();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var isSchemaRegistered = await _authRepository.RegisterUserToSchemaAsync(
                userId,
                request.Email,
                hashedPassword,
                request.Phone,
                request.FirstName,
                request.LastName,
                preferredLanguage,
                referredBy,
                _unitOfWork.Connection,
                _unitOfWork.Transaction
            );

            if (!isSchemaRegistered)
            {
                throw new BusinessRulesException(SharedResourceKeys.RegistrationFailed);
            }

            var isRoleInserted = await _authRepository.AddRoleToUserByRoleNameAsync(
                userId,
                role.ToString(),
                _unitOfWork.Connection,
                _unitOfWork.Transaction
            );

            if (!isRoleInserted)
            {
                throw new BusinessRulesException(SharedResourceKeys.RoleAssignmentFailed);
            }

            if (referredBy.HasValue)
            {
                var reward = new ReferralRewardEntity
                {
                    Id = Guid.NewGuid(),
                    ReferrerUserId = referredBy.Value,
                    ReferredUserId = userId,
                    RewardStatus = ReferralRewardStatus.PENDING.ToString(),
                    TriggeredEvent = "REGISTRATION",
                    CreatedAt = DateTime.UtcNow
                };

                await _referralRewardRepository.CreateAsync(reward, _unitOfWork.Connection, _unitOfWork.Transaction);
            }

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw new BusinessRulesException(SharedResourceKeys.RegistrationFailed);
        }

        var authRequest = new AuthRequest
        {
            Username = request.Email,
            Password = request.Password
        };

        return await AuthenticateWithPasswordAsync(authRequest, string.Empty);
    }

    protected async Task<AuthResponse> CreateAuthResponseAsync(AccountUserEntity accountUser, string? createdByIp = null)
    {
        var loginUserInfo = await _authRepository.GetWithRoleBySchemaAsync(accountUser.Email, accountUser.SchemaName);

        if (loginUserInfo is null)
        {
            throw new NotFoundException(SharedResourceKeys.UserNotFound, accountUser.Email.ToString());
        }

        if (loginUserInfo.Status != "ACTIVE")
        {
            throw new ForbiddenException();
        }

        var (token, expireDate) = await _tokenService.GenerateJwtTokenByEmail(accountUser.Email);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpireAt = DateTime.Now.AddDays(_tokenManagerConfig.RefreshTokenExpireDays);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _authRepository.InsertRefreshTokenAsync(
                accountUser.Id,
                refreshToken,
                refreshTokenExpireAt,
                accountUser.SchemaName,
                createdByIp,
                _unitOfWork.Connection,
                _unitOfWork.Transaction
            );

            await _authRepository.UpdateLastLoginDateAsync(
                accountUser.Email,
                accountUser.SchemaName,
                _unitOfWork.Connection,
                _unitOfWork.Transaction
            );

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw new BusinessRulesException(SharedResourceKeys.DbOperationFailed);
        }

        return new AuthResponse
        {
            User = new UserForAuth
            {
                Id = loginUserInfo.Id,
                FirstName = loginUserInfo.Name,
                LastName = loginUserInfo.Surname,
                Email = loginUserInfo.Email,
                UserImagePath = loginUserInfo.ImagePath,
                SchemaName = accountUser.SchemaName,
                Role = new Role()
                {
                    Id = loginUserInfo.RoleId,
                    Name = loginUserInfo.RoleName,
                    Description = loginUserInfo.RoleDescription,
                    PermissionNamesRaw = loginUserInfo.PermissionNamesRaw

                }
            },
            Token = token,
            RefreshToken = refreshToken,
            TokenExpireDate = expireDate,
            RefreshExpireDate = refreshTokenExpireAt
        };
    }

    protected UsernameLoginType UsernameTypeValidation(string username)
    {
        var phoneRegex = new Regex(@"^\+(\d{1,3})(\d{10})$");
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        if (phoneRegex.IsMatch(username))
        {
            return UsernameLoginType.Phone;
        }
        if (emailRegex.IsMatch(username))
        {
            return UsernameLoginType.Email;
        }

        return UsernameLoginType.NoType;
    }
}
