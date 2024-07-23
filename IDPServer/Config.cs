﻿using Duende.IdentityServer.Models;
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
                ClientSecrets = { new Secret(GenerateSecret()) }, // Ensure to replace with the actual secret
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
                ClientSecrets = { new Secret(GenerateSecret()) }, // Ensure to replace with the actual secret
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
                    ClientSecrets = { new Secret(GenerateSecret()) },
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
                    ClientId = "SapPlus.CompanyAPIPublish",
                    ClientSecrets = { new Secret(GenerateSecret()) },
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
                    ClientName = "SapPlus.CompanyAPIPublish",
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
                    ClientSecrets = { new Secret(GenerateSecret()) },
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


    private static string GenerateSecret()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
