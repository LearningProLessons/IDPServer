using Duende.IdentityServer.Models;
using System.Security.Cryptography;

namespace IDPServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        ];

    public static IEnumerable<Client> Clients =>
        [
         // TODO: remove this client after merge with sapplus
         // New Client: Sample.UI
         new Client
         {
             ClientId = "Sample.UI",
             ClientSecrets = { new Secret("K8T1L7J9V0D3R+4W6Fz5X2Q8B1N7P3C4G0A9J7R8H6=".Sha256()) },
             AllowedGrantTypes = GrantTypes.Code,
             RequirePkce = true,
             AllowOfflineAccess = true,
             RedirectUris = { "https://localhost:7076/signin-oidc" },
             FrontChannelLogoutUri = "https://localhost:7076/signout-oidc",
             PostLogoutRedirectUris = { "https://localhost:7076/signout-callback" }, // This should be set correctly
             AllowedScopes = { "openid", "profile", "scope1" },
             AccessTokenLifetime = 3600, // 1 hour
             IdentityTokenLifetime = 300, // 5 minutes
             AbsoluteRefreshTokenLifetime = 2592000, // 30 days
             SlidingRefreshTokenLifetime = 1296000, // 15 days
             RequireClientSecret = true,
             ClientName = "Sample.UI",
             RequireConsent = false,
             AllowRememberConsent = true,
             AlwaysIncludeUserClaimsInIdToken = false,
             AllowAccessTokensViaBrowser = false,
             Enabled = true
         }
    ];
}
