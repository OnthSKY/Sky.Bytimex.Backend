namespace Sky.Template.Backend.Core.Helpers;

public static class ServiceTool
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void Create(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}
