namespace Sky.Template.Backend.Core.Context;
public static class GlobalSchema
{
    private static AsyncLocal<string> _schemaName = new();

    public static string Name
    {
        get => _schemaName.Value ?? "sys";
        set => _schemaName.Value = value;
    }
}
