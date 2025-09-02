using System.Reflection;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using FluentValidation;
using Sky.Template.Backend.Application.Validators.FluentValidation.Auth;
using Sky.Template.Backend.Core.Aspects.Autofac.Validation;
using Sky.Template.Backend.Core.Utilities.Interceptors;
using Module = Autofac.Module;

namespace Sky.Template.Backend.Application.DependencyResolvers.Autofac;

public class AutofacApplicationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var applicationAssembly = Assembly.Load("Sky.Template.Backend.Application");

        var validatorsAssembly = typeof(AuthWithoutPasswordRequestValidator).Assembly;

        builder.RegisterAssemblyTypes(validatorsAssembly, applicationAssembly)
            .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .InstancePerDependency();

        builder.RegisterAssemblyTypes(applicationAssembly)
            .Where(t => !t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .EnableInterfaceInterceptors(new ProxyGenerationOptions
            {
                Selector = new AspectInterceptorSelector()
            })
            .InstancePerLifetimeScope();

        builder.RegisterType<ValidationAspect>().InstancePerLifetimeScope();
    }
}

