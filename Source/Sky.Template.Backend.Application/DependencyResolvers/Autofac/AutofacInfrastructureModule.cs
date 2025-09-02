using System.Reflection;
using Autofac;
using Sky.Template.Backend.Core.Encryption;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;
using Sky.Template.Backend.Infrastructure.Azure;
using Sky.Template.Backend.Infrastructure.Caching;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Sky.Template.Backend.Core.Configs;
using Module = Autofac.Module;

namespace Sky.Template.Backend.Application.DependencyResolvers.Autofac;

public class AutofacInfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var infrastructureAssembly = Assembly.Load("Sky.Template.Backend.Infrastructure");

        builder.RegisterAssemblyTypes(infrastructureAssembly)
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();


        builder.RegisterType<QueueClientProvider>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.RegisterType<AesEncryptionService>()
                .As<IEncryptionService>()
                .SingleInstance();

        builder.Register(c =>
        {
            var config = c.Resolve<IOptions<CacheConfig>>().Value;
            return config.Provider.ToLower() switch
            {
                "memory" => (ICacheProvider)new MemoryCacheProvider(c.Resolve<IMemoryCache>()),
                _ => new RedisCacheProvider(c.Resolve<IConnectionMultiplexer>())
            };
        }).As<ICacheProvider>().SingleInstance();

        builder.RegisterType<CacheService>()
            .As<ICacheService>()
            .SingleInstance();


        builder.RegisterType<CacheKeyGenerator>()
            .As<ICacheKeyGenerator>()
            .SingleInstance();

    }
}
