namespace Sky.Template.Backend.Core.Context;

public static class GlobalImpersonationContext
{
    // Hiçbir gerçek kullanýcýya atanmayacak sabit GUID
    public static readonly Guid SentinelAdminGuid = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid SentinelAdminRemovalGuid = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private static AsyncLocal<Guid> _adminId = new() { Value = SentinelAdminGuid };

    public static Guid AdminId
    {
        get => _adminId.Value;
        set => _adminId.Value = value;
    }

    public static bool IsImpersonating => AdminId != SentinelAdminGuid;

    public static Guid GetSentinelValue() => SentinelAdminGuid;
    public static Guid GetRemovalSentinelValue() => SentinelAdminRemovalGuid;
}
