namespace Sky.Template.Backend.Core.Exceptions;

public class MaintenanceModeException : Exception
{
    public MaintenanceModeException() : base("MaintenanceModeEnabled")
    {
    }
}

