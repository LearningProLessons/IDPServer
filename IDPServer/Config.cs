using Duende.IdentityServer.Models;
using System.Collections.Generic;
using System.Linq;

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
            new IdentityResource("organization_claim", "سازمان", new List<string> { "organization_id" }) ,
            new IdentityResource("roles", "User roles", new List<string> { "role" }), // Identity resource for roles
            new IdentityResource("name", "User name", new List<string> { "name" }) // Identity resource for user 
        };

    // Method to generate read and write scopes
    public static IEnumerable<ApiScope> GenerateScopes(string baseString)
    {
        return new List<ApiScope>
        {
            new ApiScope($"{baseString}.read", $"Read access for {baseString}"),
            new ApiScope($"{baseString}.write", $"Write access for {baseString}")
        };
    }

    public static IEnumerable<ApiScope> ApiScopes =>
      new List<ApiScope>()
      {
           new ApiScope("all.read", "Read access for all resources"),
           new ApiScope("all.write", "Write access for all resources")
      }
      .Concat(GenerateScopes("dashboard")) // Scopes for Dashboard
      .Concat(GenerateScopes("broadcastingCompanies")) // Scopes for Broadcasting Companies
      .Concat(GenerateScopes("companyCustomers")) // Scopes for Company Customers
      .Concat(GenerateScopes("draw")) // Scopes for Draw
      .Concat(GenerateScopes("factor")) // Scopes for Factor
      .Concat(GenerateScopes("orders")) // Scopes for Orders
      .Concat(GenerateScopes("usersManagement")); // Scopes for Users Management

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
                "roles", // Include roles scope
                "name", // Include name scope
                "all.read",
                "all.write",
                "dashboard.read",
                "dashboard.write",
                "broadcastingCompanies.read",
                "broadcastingCompanies.write",
                "companyCustomers.read",
                "companyCustomers.write",
                "draw.read",
                "draw.write",
                "factor.read",
                "factor.write",
                "orders.read",
                "orders.write",
                "usersManagement.read",
                "usersManagement.write",
            },
            AccessTokenLifetime = 3600, // 1 hour
            IdentityTokenLifetime = 300, // 5 minutes
            AbsoluteRefreshTokenLifetime = 2592000, // 30 days
            SlidingRefreshTokenLifetime = 1296000, // 15 days
            RequireClientSecret = true,
            ClientName = "NILL_Developers",
            RequireConsent = false,
            AllowRememberConsent = true,
            AlwaysIncludeUserClaimsInIdToken = true, // Ensure claims are included
            AllowAccessTokensViaBrowser = false,
        }
    };
}
