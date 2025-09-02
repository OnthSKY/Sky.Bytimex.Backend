using Castle.DynamicProxy;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Validation;
using Sky.Template.Backend.Core.Helpers;
using Sky.Template.Backend.Core.Utilities.Interceptors;
using Sky.Template.Backend.Core.Exceptions;

namespace Sky.Template.Backend.Core.Aspects.Autofac.Validation;

public class ValidationAspect : MethodInterception
{
    private readonly Type _validatorType;

    public ValidationAspect(Type validatorType)
    {
        if (!typeof(IValidator).IsAssignableFrom(validatorType))
        {
            throw new Exception("NotAValidationClass");
        }
        _validatorType = validatorType;
    }

    protected override void OnBefore(IInvocation invocation)
    {
        var validator = (IValidator)ActivatorUtilities.CreateInstance(ServiceTool.ServiceProvider, _validatorType);
        if (validator == null)
        {
            throw new BusinessRulesException("ValidatorNotResolved", _validatorType.FullName ?? string.Empty);
        }
        var entityType = _validatorType.BaseType.GetGenericArguments()[0];
        var entities = invocation.Arguments.Where(t => t.GetType() == entityType);
        foreach (var entity in entities)
        {
            ValidationTool.Validate(validator, entity);
        }
    }
}
