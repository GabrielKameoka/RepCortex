namespace RepCortex.Infrastructure.Security;

public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string PublicIngestOnly = "PublicIngestOnly";
    public const string SecretIntegrationOnly = "SecretIntegrationOnly";
}
