using Duende.IdentityServer.Models;
using Microsoft.IdentityModel.Tokens;

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
        new ApiScope("APIScope"),
        new ApiScope("UIScope"),
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new("APIScope", "Sample API") {
            Scopes = { "APIScope" },
            //AllowedAccessTokenSigningAlgorithms = { SecurityAlgorithms.RsaSsaPssSha256 } ,
            //Description = "This will access to API project to be verified in IDP"    ,
            //Enabled = true,
            //ShowInDiscoveryDocument = true,
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        // New Client: Sample.API
        // m2m client credentials flow client
        new Client
        {
            ClientId = "Sample.API",
            ClientName = "Sample.API",
            ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes = { "APIScope" }
        },

         // TODO: remove this client after merge with sapplus
         // New Client: Sample.UI
         new Client
         {
             ClientId = "Sample.UI",
             ClientName = "Sample.UI",
             ClientSecrets = { new Secret("K8T1L7J9V0D3R+4W6Fz5X2Q8B1N7P3C4G0A9J7R8H6=".Sha256()) },
             AllowedGrantTypes = GrantTypes.Code,
             RequirePkce = true,
             AllowOfflineAccess = true,
             RedirectUris = { "https://localhost:7076/signin-oidc" },
             FrontChannelLogoutUri = "https://localhost:7076/signout-oidc",
             PostLogoutRedirectUris = { "https://localhost:7076/signout-callback" }, // This should be set correctly
             AllowedScopes = { "openid", "profile", "UIScope" },
             AccessTokenLifetime = 3600, // 1 hour
             IdentityTokenLifetime = 300, // 5 minutes
             AbsoluteRefreshTokenLifetime = 2592000, // 30 days
             SlidingRefreshTokenLifetime = 1296000, // 15 days
             RequireClientSecret = true,
             RequireConsent = false,
             AllowRememberConsent = true,
             AlwaysIncludeUserClaimsInIdToken = false,
             AllowAccessTokensViaBrowser = false,
             Enabled = true
         }
    ];
}
