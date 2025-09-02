using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Core.Utilities.Interceptors;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class CacheableAttributeTests
{
    private class InnerService
    {
        [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductsPrefix), ExpirationInMinutes = 1)]
        public virtual Task<int> GetAsync() => Task.FromResult(42);
    }

    private class OuterService
    {
        private readonly InnerService _inner;
        public OuterService(InnerService inner) => _inner = inner;

        [Cacheable(CacheKeyPrefix = nameof(CacheKeys.ProductsPrefix), ExpirationInMinutes = 1)]
        public virtual Task<int> GetAsync() => _inner.GetAsync();
    }

    //[Fact]
    //public async Task NestedCacheableAttributes_InvokeCacheOnce()
    //{
    //    var cacheMock = new Mock<ICacheService>();
    //    cacheMock
    //        .Setup(s => s.GetOrSetAsync<int>(It.IsAny<string>(), It.IsAny<Func<Task<int>>>(), It.IsAny<CacheEntryOptions>()))
    //        .Returns<string, Func<Task<int>>, CacheEntryOptions>((_, factory, __) => factory());

    //    var services = new ServiceCollection();
    //    services.AddSingleton(cacheMock.Object);
    //    Sky.Template.Backend.Core.Helpers.ServiceTool.ServiceProvider = services.BuildServiceProvider();

    //    var generator = new ProxyGenerator();
    //    var options = new ProxyGenerationOptions { Selector = new AspectInterceptorSelector() };
    //    var inner = generator.CreateClassProxy<InnerService>(options, Array.Empty<IInterceptor>());
    //    var outer = generator.CreateClassProxy<OuterService>(options, Array.Empty<IInterceptor>(), inner);

    //    var result = await outer.GetAsync();
    //    result.Should().Be(42);

    //    cacheMock.Verify(s => s.GetOrSetAsync<int>(It.IsAny<string>(), It.IsAny<Func<Task<int>>>(), It.IsAny<CacheEntryOptions>()), Times.Once);
    //}
}
