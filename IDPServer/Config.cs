﻿using Duende.IdentityServer.Models;
using static System.Net.WebRequestMethods;

namespace IDPServer;

public static class Config
{
    public static IEnumerable<string> Organizations => new List<string>
    {
        // Your existing organization names
        "شرکت پخش پگاه",
        "شرکت لینا",
        "شرکت فیروز",
        "صنایع غذایی میهن - بستنی میهن",
        "صنایع غذایی میهن - لبنیات پاستوریزه میهن",
        "صنایع غذایی میهن - لبنیات استریل میهن",
        "صنایع غذایی میهن - بستنی پاندا",
        "صنایع غذایی میهن - لبنیات استریل پاندا",
        "صنایع غذایی میهن - فروت لند",
        "صنایع غذایی میهن - لبنیات برنارد",
        "صنایع غذایی میهن - بستنی برنارد",
        "صنایع غذایی میهن - غذا فرآور پاندا",
        "صنایع غذایی میهن - آلینوس",
        "شرکت محمد نظری",
        "شرکت جدید اکبری",
        "کومپانا",
        "سیب",
        "شرکت محمدرضا",
        "شرکت محمدرضا1",
        "شرکت 3",
        "شرکت میلاد",
        "گروه توسعه نیل",
        "شرکت همراه اوپت"
    };

    public static IEnumerable<IdentityResource> IdentityResources =>
    new List<IdentityResource>
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource("organization_claim", "سازمان", new List<string> { "organization" }) // Renamed identity scope
    };

    public static IEnumerable<ApiScope> ApiScopes =>
    new List<ApiScope>
    {
        new ApiScope("all.read", "Read access for all resources"),
        new ApiScope("all.write", "Write access for all resources"),
        new ApiScope("order.read", "Read access for Order"),
        new ApiScope("order.write", "Write access for Order"),
    };

    public static IEnumerable<Client> Clients =>
    new List<Client>
    {
        new Client
        {
            ClientId = "NILLClient",
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
                "all.read",
                "all.write",
                "order.read",
                "order.write"
            },
            AccessTokenLifetime = 3600, // 1 hour
            IdentityTokenLifetime = 300, // 5 minutes
            AbsoluteRefreshTokenLifetime = 2592000, // 30 days
            SlidingRefreshTokenLifetime = 1296000, // 15 days
            RequireClientSecret = true,
            ClientName = "NILL_Developers",
            RequireConsent = false,
            AllowRememberConsent = true,
            AlwaysIncludeUserClaimsInIdToken = false,
            AllowAccessTokensViaBrowser = false,
        }
    };
}
