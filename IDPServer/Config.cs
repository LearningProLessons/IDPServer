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
            // New Client: SapPlus.CompanyUI
            new Client
            {
                ClientId = "SapPlus.CompanyUI",
                ClientSecrets = { new Secret("6H9kVtOaXAlZcA1bIkPjMfR62RyGnA9QsPUz3w5y4ow=".Sha256()) }, // Ensure to replace with the actual secret
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                AllowOfflineAccess = true,
                RedirectUris = { "https://localhost:64024/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:64024/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:64024/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile" },
                AccessTokenLifetime = 3600, // 1 hour
                IdentityTokenLifetime = 300, // 5 minutes
                AbsoluteRefreshTokenLifetime = 2592000, // 30 days
                SlidingRefreshTokenLifetime = 1296000, // 15 days
                RequireClientSecret = true,
                ClientName = "SapPlus.CompanyUI",
                RequireConsent = false,
                AllowRememberConsent = true,
                AlwaysIncludeUserClaimsInIdToken = false,
                AllowAccessTokensViaBrowser = false,
                Enabled = true
            },

            // New Client: SapPlus.CompanyUI.Publish
            new Client
            {
                ClientId = "SapPlus.CompanyUI.Publish",
                ClientSecrets = { new Secret("mNc7zKzYlLrjA9FzF4L1oOe5RZ9x2H6h1jK7Pq8zW5Q=".Sha256()) }, // Ensure to replace with the actual secret
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                AllowOfflineAccess = true,
                RedirectUris = { "https://sapplus.ir:58842/signin-oidc" },
                FrontChannelLogoutUri = "https://sapplus.ir:58842/signout-oidc",
                PostLogoutRedirectUris = { "https://sapplus.ir:58842/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile" },
                AccessTokenLifetime = 3600, // 1 hour
                IdentityTokenLifetime = 300, // 5 minutes
                AbsoluteRefreshTokenLifetime = 2592000, // 30 days
                SlidingRefreshTokenLifetime = 1296000, // 15 days
                RequireClientSecret = true,
                ClientName = "SapPlus.CompanyUI.Publish",
                RequireConsent = false,
                AllowRememberConsent = true,
                AlwaysIncludeUserClaimsInIdToken = false,
                AllowAccessTokensViaBrowser = false,
                Enabled = true
            } ,

             // New Client: SapPlus.CompanyAPI
                new Client
                {
                    ClientId = "SapPlus.CompanyAPI",
                    ClientSecrets = { new Secret("Yj2D1VuN4X2zPcM3rK8vI6U0YpL9QsWxVfO7e4r6t8w=".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    AllowOfflineAccess = true,
                    RedirectUris = { "https://localhost:58862/signin-oidc", "https://sapplus.ir:58843/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:58862/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:58862/signout-callback-oidc", "https://sapplus.ir:58843/signout-callback-oidc" },
                    AllowedScopes = { "openid", "profile" },
                    AccessTokenLifetime = 3600, // 1 hour
                    IdentityTokenLifetime = 300, // 5 minutes
                    AbsoluteRefreshTokenLifetime = 2592000, // 30 days
                    SlidingRefreshTokenLifetime = 1296000, // 15 days
                    RequireClientSecret = true,
                    ClientName = "SapPlus.CompanyAPI",
                    RequireConsent = false,
                    AllowRememberConsent = true,
                    AlwaysIncludeUserClaimsInIdToken = false,
                    AllowAccessTokensViaBrowser = false,
                    Enabled = true
                },

                // New Client: SapPlus.CompanyAPIPublish
                new Client
                {
                    ClientId = "SapPlus.CompanyAPI.Publish",
                    ClientSecrets = { new Secret("oA3vY6mGzR1P4zK2Lx5Fq9O8h7D0NwQ4jE2kN1v6Rg=".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    AllowOfflineAccess = true,
                    RedirectUris = { "https://sapplus.ir:58843/signin-oidc" },
                    FrontChannelLogoutUri = "https://sapplus.ir:58843/signout-oidc",
                    PostLogoutRedirectUris = { "https://sapplus.ir:58843/signout-callback-oidc" },
                    AllowedScopes = { "openid", "profile", "scope2" },
                    AccessTokenLifetime = 3600, // 1 hour
                    IdentityTokenLifetime = 300, // 5 minutes
                    AbsoluteRefreshTokenLifetime = 2592000, // 30 days
                    SlidingRefreshTokenLifetime = 1296000, // 15 days
                    RequireClientSecret = true,
                    ClientName = "SapPlus.CompanyAPI.Publish",
                    RequireConsent = false,
                    AllowRememberConsent = true,
                    AlwaysIncludeUserClaimsInIdToken = false,
                    AllowAccessTokensViaBrowser = false,
                    Enabled = true
                }    ,


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
                    PostLogoutRedirectUris = { "https://localhost:7076/signout-callback-oidc" },
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
