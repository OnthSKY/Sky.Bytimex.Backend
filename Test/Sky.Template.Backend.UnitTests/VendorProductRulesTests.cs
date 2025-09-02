using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq;
using System.Data.Common;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Sky.Template.Backend.Application.Services.System;
using Sky.Template.Backend.Application.Services.Vendor;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Utilities.Interceptors;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.System;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class VendorProductRulesTests
{
    private static IHttpContextAccessor CreateAccessor(Guid? vendorId = null)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var ctx = new DefaultHttpContext();
        var id = vendorId ?? Guid.NewGuid();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) }));
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return accessor.Object;
    }

    private class TestVendorProductService : VendorProductService
    {
        public int ActiveProductCount { get; set; }
        public string? VendorKycStatus { get; set; }
        public ProductEntity? InsertedProduct { get; private set; }

        public TestVendorProductService(
            ISettingsService settingsService,
            IProductSettingsService productSettingsService,
            IHttpContextAccessor accessor,
            IProductRepository productRepository,
            IProductTranslationRepository translationRepository,
            IVendorRepository vendorRepository)
            : base(productRepository, translationRepository, vendorRepository, accessor, settingsService, productSettingsService, CreateUnitOfWork().Object)
        {
        }

        private static Mock<IUnitOfWork> CreateUnitOfWork()
        {
            var uow = new Mock<IUnitOfWork>();
            uow.SetupGet(u => u.Connection).Returns(new Mock<DbConnection>().Object);
            uow.SetupGet(u => u.Transaction).Returns(new Mock<DbTransaction>().Object);
            uow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            uow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            uow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
            return uow;
        }

        protected override Task<int> GetActiveProductCountAsync(Guid vendorId)
            => Task.FromResult(ActiveProductCount);

        protected override Task<string?> GetVendorKycStatusAsync(Guid vendorId)
            => Task.FromResult(VendorKycStatus);

        protected override Task InsertProductAsync(ProductEntity p, string name, string? desc, string lang)
        {
            InsertedProduct = p;
            return Task.CompletedTask;
        }
    }

    private static TestVendorProductService CreateServiceWithRepos(
        ISettingsService settings,
        IProductSettingsService productSettings,
        IHttpContextAccessor accessor,
        out Mock<IProductRepository> productRepoMock,
        out Mock<IProductTranslationRepository> translationRepoMock,
        out Mock<IVendorRepository> vendorRepoMock)
    {
        productRepoMock = new Mock<IProductRepository>();
        // SKU her zaman unique (testlerde engel olmasın)
        productRepoMock.Setup(r => r.IsSkuUniqueAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
                       .ReturnsAsync(true);

        translationRepoMock = new Mock<IProductTranslationRepository>();
        vendorRepoMock = new Mock<IVendorRepository>();

        return new TestVendorProductService(settings, productSettings, accessor,
            productRepoMock.Object, translationRepoMock.Object, vendorRepoMock.Object);
    }

    [Fact]
    public async Task MaintenanceMode_BlocksCreation()
    {
        var provider = new Mock<ISettingsService>();
        provider.Setup(p => p.GetSettingsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                {"MAINTENANCE_MODE", "true"},
                {"MAX_PRODUCT_COUNT_PER_VENDOR", "10"},
                {"REQUIRE_VENDOR_KYC_FOR_PUBLISHING", "false"}
            });

        var service = CreateServiceWithRepos(provider.Object, new Mock<IProductSettingsService>().Object, CreateAccessor(),
            out _, out _, out _);

        await Assert.ThrowsAsync<MaintenanceModeException>(() =>
            service.CreateProductAsync(new CreateProductRequest { Name = "A", ProductType = "PHYSICAL", Price = 1M, Status = "ACTIVE" }));
    }

    [Fact]
    public async Task ProductLimitExceeded_Throws()
    {
        var provider = new Mock<ISettingsService>();
        provider.Setup(p => p.GetSettingsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                {"MAINTENANCE_MODE", "false"},
                {"MAX_PRODUCT_COUNT_PER_VENDOR", "1"},
                {"REQUIRE_VENDOR_KYC_FOR_PUBLISHING", "false"}
            });

        var service = CreateServiceWithRepos(provider.Object, new Mock<IProductSettingsService>().Object, CreateAccessor(),
            out _, out _, out _);
        service.ActiveProductCount = 1;

        await Assert.ThrowsAsync<MaxProductLimitExceededException>(() =>
            service.CreateProductAsync(new CreateProductRequest { Name = "A", ProductType = "PHYSICAL", Price = 1M, Status = "ACTIVE" }));
    }

    [Fact]
    public async Task KycRequiredForPublishing_Throws()
    {
        var provider = new Mock<ISettingsService>();
        provider.Setup(p => p.GetSettingsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                {"MAINTENANCE_MODE", "false"},
                {"MAX_PRODUCT_COUNT_PER_VENDOR", "10"},
                {"REQUIRE_VENDOR_KYC_FOR_PUBLISHING", "true"}
            });

        var service = CreateServiceWithRepos(provider.Object, new Mock<IProductSettingsService>().Object, CreateAccessor(),
            out _, out _, out _);
        service.VendorKycStatus = "PENDING";

        await Assert.ThrowsAsync<KycRequiredException>(() =>
            service.CreateProductAsync(new CreateProductRequest { Name = "A", ProductType = "PHYSICAL", Price = 1M, Status = "PUBLISHED" }));
    }

    [Fact]
    public async Task StatusForcedToDraft_WhenKycNotVerified()
    {
        var provider = new Mock<ISettingsService>();
        provider.Setup(p => p.GetSettingsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new Dictionary<string, string>
            {
                {"MAINTENANCE_MODE", "false"},
                {"MAX_PRODUCT_COUNT_PER_VENDOR", "10"},
                {"REQUIRE_VENDOR_KYC_FOR_PUBLISHING", "true"}
            });

        var service = CreateServiceWithRepos(provider.Object, new Mock<IProductSettingsService>().Object, CreateAccessor(),
            out _, out _, out _);
        service.VendorKycStatus = "PENDING";

        var res = await service.CreateProductAsync(new CreateProductRequest { Name = "A", ProductType = "PHYSICAL", Price = 1M, Status = "ACTIVE" });
        res.Data!.Status.Should().Be("DRAFT");
        service.InsertedProduct!.Status.Should().Be("DRAFT");
    }

    [Fact]
    public async Task UpdateProduct_KycRequired_Throws()
    {
        var vendorId = Guid.NewGuid();

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ProductEntity { Id = Guid.NewGuid(), VendorId = vendorId, CreatedBy = vendorId });
        productRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<DbConnection>(), It.IsAny<DbTransaction>()))
            .Returns(Task.CompletedTask);

        var translationRepo = new Mock<IProductTranslationRepository>();
        var vendorRepo = new Mock<IVendorRepository>();
        vendorRepo.Setup(v => v.GetKycStatusAsync(It.IsAny<Guid>(), It.IsAny<DbConnection>(), It.IsAny<DbTransaction>()))
            .ReturnsAsync("PENDING");

        var uow = new Mock<IUnitOfWork>();
        uow.SetupGet(u => u.Connection).Returns(new Mock<DbConnection>().Object);
        uow.SetupGet(u => u.Transaction).Returns(new Mock<DbTransaction>().Object);
        uow.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        uow.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        uow.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var settingsService = new Mock<ISettingsService>();
        var productSettings = new Mock<IProductSettingsService>();
        productSettings.Setup(s => s.GetEffectiveSettingsAsync(vendorId))
            .ReturnsAsync(new ProductSettings
            {
                MaintenanceMode = false,
                MaxProductCountPerVendor = 10,
                RequireVendorKycForPublishing = true,
                AllowProductDeletion = true
            });

        var service = new VendorProductService(productRepo.Object, translationRepo.Object, vendorRepo.Object,
            CreateAccessor(vendorId), settingsService.Object, productSettings.Object, uow.Object);

        var req = new UpdateProductRequest { Name = "x", ProductType = "PHYSICAL", Price = 1M, Status = "PUBLISHED", IsStockTracked = true, StockQuantity = 1, Sku = "s" };

        await Assert.ThrowsAsync<KycRequiredException>(() => service.UpdateProductAsync(Guid.NewGuid(), req));
    }

    [Fact]
    public async Task SoftDelete_Blocked_WhenNotAllowed()
    {
        var vendorId = Guid.NewGuid();
        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ProductEntity { Id = Guid.NewGuid(), VendorId = vendorId, CreatedBy = vendorId });
        productRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductEntity>())).ReturnsAsync(new ProductEntity());

        var productSettings = new Mock<IProductSettingsService>();
        productSettings.Setup(s => s.GetEffectiveSettingsAsync(vendorId))
            .ReturnsAsync(new ProductSettings { MaintenanceMode = false, AllowProductDeletion = false, MaxProductCountPerVendor = 10, RequireVendorKycForPublishing = false });

        var service = new VendorProductService(productRepo.Object, new Mock<IProductTranslationRepository>().Object, new Mock<IVendorRepository>().Object,
            CreateAccessor(vendorId), new Mock<ISettingsService>().Object, productSettings.Object, new Mock<IUnitOfWork>().Object);

        await Assert.ThrowsAsync<BusinessRulesException>(() => service.SoftDeleteProductAsync(Guid.NewGuid()));
    }

    private class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _store = new();

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheEntryOptions options)
        {
            if (_store.TryGetValue(key, out var value))
                return Task.FromResult((T)value);
            return HandleAsync();

            async Task<T> HandleAsync()
            {
                var result = await factory();
                _store[key] = result!;
                return result;
            }
        }

        public Task RemoveAsync(string key)
        {
            _store.Remove(key);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
            => Task.FromResult<IEnumerable<string>>(_store.Keys);
    }

    [Fact]
    public async Task SettingsService_UsesCacheWithinTtl()
    {
        var repo = new Mock<ISettingRepository>();
        repo.Setup(r => r.GetSettingsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync((IEnumerable<string> keys) => keys.ToDictionary(k => k, _ => "false"));

        var svc = new SettingsService(repo.Object, new Mock<IHttpContextAccessor>().Object);

        var services = new ServiceCollection();
        services.AddSingleton<ICacheService>(new FakeCacheService());
        ServiceTool.Create(services.BuildServiceProvider());

        var generator = new ProxyGenerator();
        var proxy = generator.CreateInterfaceProxyWithTarget<ISettingsService>(svc, new ProxyGenerationOptions
        {
            Selector = new AspectInterceptorSelector()
        });

        await proxy.GetSettingsAsync(new[] { "MAINTENANCE_MODE" });
        await proxy.GetSettingsAsync(new[] { "MAINTENANCE_MODE" });

        repo.Verify(r => r.GetSettingsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
    }
}
