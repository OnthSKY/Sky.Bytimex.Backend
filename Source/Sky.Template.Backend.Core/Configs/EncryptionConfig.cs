namespace Sky.Template.Backend.Core.Configs;

public class EncryptionConfig
{
    public string ApiIntegrationEncryptionKey { get; set; } = null!;       // DB �ifreleme i�in
    public string MobileCustomAuthKey { get; set; } = null!;    // Mobil �zel auth i�in
}
