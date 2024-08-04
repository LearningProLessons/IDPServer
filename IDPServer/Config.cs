using Duende.IdentityServer.Models;


namespace IDPServer;

public static class Config
{
    public static IEnumerable<string> Organizations => new List<string>
    {
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
        new IdentityResource("organization", "سازمان", new List<string> { "organization" })
    };

    public static IEnumerable<ApiScope> ApiScopes =>
    new List<ApiScope>
    {
        #region Admin Scope
        new ApiScope("all.read", "Read access for all resources"),
        new ApiScope("all.write", "Write access for all resources"),
        #endregion

        new ApiScope("broadcastingcompanies.companycustomers.read", "Read access for Company Customers"),
        new ApiScope("broadcastingcompanies.companycustomers.write", "Write access for Company Customers"),
        new ApiScope("companycustomer.read", "Read access for Company Customer"),
        new ApiScope("companycustomer.write", "Write access for Company Customer"),
        new ApiScope("orderdetails.read", "Read access for Order Details"),
        new ApiScope("orderdetails.write", "Write access for Order Details"),
        new ApiScope("orders.read", "Read access for Orders"),
        new ApiScope("orders.write", "Write access for Orders"),
        new ApiScope("dashboard.broadcastingcompanies.companycustomers.read", "Read access for Dashboard Company Customers"),
        new ApiScope("dashboard.broadcastingcompanies.companycustomers.write", "Write access for Dashboard Company Customers"),
        new ApiScope("dashboard.companycustomer.read", "Read access for Dashboard Company Customer"),
        new ApiScope("dashboard.companycustomer.write", "Write access for Dashboard Company Customer"),
        new ApiScope("dashboard.draw.read", "Read access for Draw"),
        new ApiScope("dashboard.draw.write", "Write access for Draw"),
        new ApiScope("dashboard.factor.read", "Read access for Factor"),
        new ApiScope("dashboard.factor.write", "Write access for Factor"),
        new ApiScope("dashboard.orders.read", "Read access for Dashboard Orders"),
        new ApiScope("dashboard.orders.write", "Write access for Dashboard Orders"),
        new ApiScope("dashboard.usersmanagement.read", "Read access for Users Management"),
        new ApiScope("dashboard.usersmanagement.write", "Write access for Users Management"),
        new ApiScope("index.read", "Read access for Index"),
        new ApiScope("index.write", "Write access for Index"),

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
            AllowedScopes = { "openid", "profile", "organization", "all.read", "all.write" }, 
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
            Enabled = true
        }
    };
}
