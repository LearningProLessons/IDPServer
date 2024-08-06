using Duende.IdentityServer.Models;


namespace IDPServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("organizations", "User organizations", new List<string> { "organizationId" }),
            new IdentityResource("roles", "User roles", new List<string> { "role" }), // Identity resource for roles
            new IdentityResource("name", "User name", new List<string> { "name" }) // Identity resource for user 
        };

  
    public static IEnumerable<ApiScope> ApiScopes =>
      new List<ApiScope>()
      {
       new ApiScope("organization.read", "Read organization data"),
       new ApiScope("organization.write", "Write organization data"),
      };


    public static IEnumerable<Client> Clients =>
    new List<Client>
    {
       new Client
        {
            ClientId = "SappPlusCompanyUIClient",
            ClientSecrets = { new Secret("K8T1L7J9V0D3R+4W6Fz5X2Q8B1N7P3C4G0A9J7R8H6=".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            RequirePkce = true,
            AllowOfflineAccess = true,
            RedirectUris = { "https://localhost:7076/signin-oidc" },
            FrontChannelLogoutUri = "https://localhost:7076/signout-oidc",
            PostLogoutRedirectUris = { "https://localhost:7076/signout-callback" },
            AllowedScopes = {
                "openid",
                "profile",
                "roles",
                "name",
                "organizations", // Include organization scope
                "organization.read", // Include organization read scope
                "organization.write", // Include organization write scope
            },
            AccessTokenLifetime = 3600, // 1 hour
            IdentityTokenLifetime = 300, // 5 minutes
            AbsoluteRefreshTokenLifetime = 2592000, // 30 days
            SlidingRefreshTokenLifetime = 1296000, // 15 days
            RequireClientSecret = true,
            ClientName = "SappPlusCompanyUI",
            RequireConsent = false,
            AllowRememberConsent = true,
            AlwaysIncludeUserClaimsInIdToken = true, // Ensure claims are included
            AllowAccessTokensViaBrowser = false,
            Enabled = true
        }
    };

    public static readonly string[] OrganizationRoles = { "PegahAdmin", "MihanAdmin" }; // Add this line

    private static IEnumerable<ApiScope> GenerateScopes(string baseString)
    {
        return new List<ApiScope>
        {
            new ApiScope($"{baseString}.read", $"Read access for {baseString}"),
            new ApiScope($"{baseString}.write", $"Write access for {baseString}")
        };
    }
}
