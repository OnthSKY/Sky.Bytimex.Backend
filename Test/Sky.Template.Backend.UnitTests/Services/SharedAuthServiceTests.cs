using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Application.Services;
using Sky.Template.Backend.Contract.Requests.Auth;
using Sky.Template.Backend.Core.Configs;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities;
using Sky.Template.Backend.Infrastructure.Entities.User;
using Sky.Template.Backend.Core.Localization;
using Sky.Template.Backend.Core.Context;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Infrastructure.Entities.Auth;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;

namespace Sky.Template.Backend.UnitTests.Services;

public class SharedAuthServiceTests
{
    private SharedAuthService CreateService(
        Mock<IAuthRepository>? authRepo = null,
        Mock<IUserRepository>? userRepo = null,
        Mock<IReferralRewardRepository>? referralRepo = null)
    {
        var authRepository = authRepo ?? new Mock<IAuthRepository>();
        var referralRepository = referralRepo ?? new Mock<IReferralRewardRepository>();
        var password = new Mock<IPasswordHashService>();
        password.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed");
        password.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        var token = new Mock<ITokenService>();
        token.Setup(t => t.GenerateJwtTokenByEmail(It.IsAny<string>())).ReturnsAsync(("t", DateTime.UtcNow));
        token.Setup(t => t.GenerateRefreshToken()).Returns("r");
        var userService = new Mock<IUserService>();
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        uow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        uow.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);
        uow.SetupGet(x => x.Connection).Returns((System.Data.Common.DbConnection)null!);
        uow.SetupGet(x => x.Transaction).Returns((System.Data.Common.DbTransaction)null!);
        var options = Options.Create(new TokenManagerConfig());

        return new SharedAuthService(authRepository.Object, options, uow.Object, password.Object, token.Object, userService.Object, referralRepository.Object);
    }

    [Fact]
    public async Task RegisterUserAsync_InvalidReferralGuid_Throws()
    {
        var service = CreateService();
        var req = new RegisterUserRequest { Email = "a@a.com", Password = "Passw0rd!", Username = "aa", FirstName = "a", LastName = "b", Phone = "+10000000000", ReferralCode = "abc" };
        var ex = await Assert.ThrowsAsync<BusinessRulesException>(() => service.RegisterUserAsync(req));
        Assert.Equal(SharedResourceKeys.ReferralsCodeInvalid, ex.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_ReferralNotFound_Throws()
    {
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(u => u.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserEntity?)null);
        var service = CreateService(userRepo: userRepo);
        var req = new RegisterUserRequest { Email = "a@a.com", Password = "Passw0rd!", Username = "aa", FirstName = "a", LastName = "b", Phone = "+10000000000", ReferralCode = Guid.NewGuid().ToString() };
        var ex = await Assert.ThrowsAsync<BusinessRulesException>(() => service.RegisterUserAsync(req));
        Assert.Equal(SharedResourceKeys.ReferralsCodeNotFound, ex.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_SelfReferral_Throws()
    {
        var id = Guid.NewGuid();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(u => u.GetByIdAsync(id)).ReturnsAsync(new UserEntity { Id = id, Email = "a@a.com" });
        var authRepo = new Mock<IAuthRepository>();
        authRepo.SetupSequence(a => a.GetSystemUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AccountUserEntity?)null)
            .ReturnsAsync(new AccountUserEntity { Email = "a@a.com", Password = "hashed", SchemaName = GlobalSchema.Name, Id = id });
        var service = CreateService(authRepo, userRepo);
        var req = new RegisterUserRequest { Email = "a@a.com", Password = "Passw0rd!", Username = "aa", FirstName = "a", LastName = "b", Phone = "+10000000000", ReferralCode = id.ToString() };
        var ex = await Assert.ThrowsAsync<BusinessRulesException>(() => service.RegisterUserAsync(req));
        Assert.Equal(SharedResourceKeys.ReferralsSelfReferralNotAllowed, ex.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_EmailDuplicate_Throws()
    {
        var authRepo = new Mock<IAuthRepository>();
        authRepo.Setup(a => a.GetSystemUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(new AccountUserEntity());
        var service = CreateService(authRepo);
        var req = new RegisterUserRequest { Email = "a@a.com", Password = "Passw0rd!", Username = "aa", FirstName = "a", LastName = "b", Phone = "+10000000000" };
        var ex = await Assert.ThrowsAsync<BusinessRulesException>(() => service.RegisterUserAsync(req));
        Assert.Equal(SharedResourceKeys.UsersEmailAlreadyExists, ex.Message);
    }

    [Fact]
    public async Task RegisterUserAsync_ReferralSuccess_CreatesReward()
    {
        var refId = Guid.NewGuid();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(u => u.GetByIdAsync(refId)).ReturnsAsync(new UserEntity { Id = refId, Email = "ref@a.com" });

        var authRepo = new Mock<IAuthRepository>();
        authRepo.SetupSequence(a => a.GetSystemUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AccountUserEntity?)null)
            .ReturnsAsync(new AccountUserEntity { Email = "b@a.com", Password = "hashed", SchemaName = GlobalSchema.Name, Id = Guid.NewGuid() });
        authRepo.Setup(a => a.RegisterUserToSchemaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<System.Data.Common.DbConnection>(), It.IsAny<System.Data.Common.DbTransaction>())).ReturnsAsync(true);
        authRepo.Setup(a => a.AddRoleToUserByRoleNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<System.Data.Common.DbConnection>(), It.IsAny<System.Data.Common.DbTransaction>())).ReturnsAsync(true);
        authRepo.Setup(a => a.GetWithRoleBySchemaAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new UserWithRoleEntity { Id = Guid.NewGuid(), Name = "X", Surname = "Y", Email = "b@a.com", RoleId = 1, RoleName = "USER", RoleDescription = "", PermissionNamesRaw = "", Status = "ACTIVE" });
        authRepo.Setup(a => a.InsertRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Data.Common.DbConnection>(), It.IsAny<System.Data.Common.DbTransaction>())).ReturnsAsync(true);
        authRepo.Setup(a => a.UpdateLastLoginDateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Data.Common.DbConnection>(), It.IsAny<System.Data.Common.DbTransaction>())).ReturnsAsync(true);

        var referralRepo = new Mock<IReferralRewardRepository>();
        referralRepo.Setup(r => r.CreateAsync(It.IsAny<ReferralRewardEntity>(), It.IsAny<System.Data.Common.DbConnection>(), It.IsAny<System.Data.Common.DbTransaction>())).Returns(Task.CompletedTask).Verifiable();

        var service = CreateService(authRepo, userRepo, referralRepo);

        var req = new RegisterUserRequest { Email = "b@a.com", Password = "Passw0rd!", Username = "bb", FirstName = "b", LastName = "c", Phone = "+10000000000", ReferralCode = refId.ToString() };

        var result = await service.RegisterUserAsync(req);

        Assert.NotNull(result);
        referralRepo.Verify(r => r.CreateAsync(It.IsAny<ReferralRewardEntity>(), It.IsAny<System.Data.Common.DbConnection>(), It.IsAny<System.Data.Common.DbTransaction>()), Times.Once);
    }
}

