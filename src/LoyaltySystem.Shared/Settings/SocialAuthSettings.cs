namespace LoyaltySystem.Shared.API.Settings;

public class SocialAuthSettings
{
    public GoogleAuthSettings Google { get; set; }
    public AppleAuthSettings Apple { get; set; }
}

public class GoogleAuthSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string[] Scopes { get; set; }
    public string[] AllowedDomains { get; set; }
}

public class AppleAuthSettings
{
    public string ClientId { get; set; }
    public string TeamId { get; set; }
    public string KeyId { get; set; }
    public string PrivateKey { get; set; }
    public string RedirectUri { get; set; }
    public string[] Scopes { get; set; }
} 