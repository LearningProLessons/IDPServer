using Duende.IdentityServer.Models;


namespace IDPServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "User roles", new[] { "role" }),
            new IdentityResource("tenant", "Tenant Id", new[] { "tenant_id" }),
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("charity_api", "Charity API")
            {
                Scopes =
                {
                    "charity.branch.read",
                    "charity.branch.write",
                    "charity.campaign.manage",
                    "charity.finance.manage",
                    "charity.client.access"
                },
                UserClaims = { "role", "tenant_id" }
            }
        };


    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("offline_access", new[] { "role", "tenant_id" }),
            new ApiScope("charity.branch.read", new[] { "role", "tenant_id" }),
            new ApiScope("charity.branch.write", new[] { "role", "tenant_id" }),
            new ApiScope("charity.campaign.manage", new[] { "role", "tenant_id" }),
            new ApiScope("charity.finance.manage", new[] { "role", "tenant_id" }),
            new ApiScope("charity.client.access", new[] { "role", "tenant_id" }),
        };


    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "SappPlusCompanyUIClient",
                ClientSecrets = { new Secret("K8T1L7J9V0D3R+4W6Fz5X2Q8B1N7P3C4G0A9J7R8H6=".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = true,
                RequirePkce = true,
                AllowAccessTokensViaBrowser = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                AllowOfflineAccess = true,
                RedirectUris = { "https://localhost:7076/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:7076/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:7076/signout-callback" },
                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "roles",
                    "tenant",
                    "offline_access",
                    "charity.branch.read",
                    "charity.branch.write",
                    "charity.campaign.manage",
                    "charity.finance.manage",
                    "charity.client.access"
                },
                AccessTokenLifetime = 3600, // 1 hour
                IdentityTokenLifetime = 300, // 5 minutes
                AbsoluteRefreshTokenLifetime = 2592000, // 30 days
                SlidingRefreshTokenLifetime = 1296000, // 15 days

                ClientName = "SappPlusCompanyUI",
                RequireConsent = false,
                AllowRememberConsent = true,
                Enabled = true
            }
        };
}