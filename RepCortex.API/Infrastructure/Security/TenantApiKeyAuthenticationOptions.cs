using Microsoft.AspNetCore.Authentication;

namespace RepCortex.Infrastructure.Security;

public sealed class TenantApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public TenantApiKeyType KeyType { get; set; }
}

public enum TenantApiKeyType
{
    Publishable = 1,
    Secret = 2
}
