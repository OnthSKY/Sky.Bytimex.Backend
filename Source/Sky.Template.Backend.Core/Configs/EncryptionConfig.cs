namespace Sky.Template.Backend.Core.Configs;

public class EncryptionConfig
{
    public string ApiIntegrationEncryptionKey { get; set; } = null!;       // DB þifreleme için
    public string MobileCustomAuthKey { get; set; } = null!;    // Mobil özel auth için
}
