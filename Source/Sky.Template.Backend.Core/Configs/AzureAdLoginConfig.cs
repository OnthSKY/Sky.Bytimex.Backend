namespace Sky.Template.Backend.Core.Configs;

public class AzureAdLoginConfig
{
    public string ClientId { get; set; }
    public string TenantId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUrl { get; set; }
    public string ScopeLink { get; set; }
    public string AuthorityLink { get; set; }
}
