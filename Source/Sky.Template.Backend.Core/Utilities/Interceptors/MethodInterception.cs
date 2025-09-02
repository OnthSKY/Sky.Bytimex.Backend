using Castle.DynamicProxy;

namespace Sky.Template.Backend.Core.Utilities.Interceptors;

public abstract class MethodInterception : MethodInterceptionBaseAttribute
{
    protected virtual void OnBefore(IInvocation invocation) { }
    protected virtual void OnAfter(IInvocation invocation) { }
    protected virtual void OnException(IInvocation invocation, Exception e) { }
    protected virtual void OnSuccess(IInvocation invocation) { }
    public override void Intercept(IInvocation invocation)
    {
        var isSuccess = true;

        OnBefore(invocation);
        var isHandled  = invocation.ReturnValue != null;
        if (!isHandled)
        {
            try
            {
                invocation.Proceed();
            }
            catch (Exception e)
            {
                isSuccess = false;
                OnException(invocation, e);
                throw;
            }

            if (isSuccess)
            {
                OnSuccess(invocation);
            }
        }

        OnAfter(invocation);
    }
}
