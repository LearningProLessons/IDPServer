using Duende.IdentityServer.Models;
using IDPServer.Constants;

namespace IDPServer;

/// <summary>
/// Central IdentityServer configuration for the e-commerce platform.
///
/// Two audiences are served by this single SSO:
///   1) Admin Panel (Next.js)      -> Authorization Code + PKCE, email + password (+ optional TOTP 2FA)
///   2) Customer App (React Native)-> Authorization Code + PKCE, mobile number + OTP
///
/// Both audiences share the SAME /Account/Login Razor page. The page decides which
/// view (password form vs OTP form) to render by reading the client's custom
/// property "login_ui" from context.Client.Properties, e.g.:
///
///   var loginUi = context?.Client?.Properties.GetValueOrDefault("login_ui") ?? "admin";
///
/// This keeps everything inside Duende's own extensibility model — no custom grant
/// type, no extra tables, no separate auth server needed for phase 1 (B2C).
///
/// Roadmap note (B2B): the "tenant" identity resource + "tenant_id" claim are already
/// wired so that when B2B rolls out, we only need to (a) populate tenant_id per user
/// and (b) add B2B-scoped ApiScopes — no breaking changes to clients defined here.
/// </summary>
public static class Config
{
    // ===================== Identity Resources =====================
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Phone(), // needed for OTP/mobile identity (phone_number, phone_number_verified)
            new IdentityResources.Email(), // needed for the admin email+password login (email, email_verified)

            new IdentityResource(
                name: "roles",
                displayName: "User roles",
                userClaims: new[] { "role" }),
            new IdentityResource(
                name: "tenant",
                displayName: "Tenant Id",
                userClaims: new[] { "tenant_id" }), // reserved for future B2B phase

            new IdentityResource(
                name: "user_type",
                displayName: "User Type (Admin/Customer)",
                userClaims: new[] { "user_type" }),
        };

    // ===================== API Scopes =====================
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("offline_access"),

            // ---- Customer-facing storefront scopes (B2C, consumed by the RN app) ----
            new ApiScope("store.catalog.read", "Browse products"),
            new ApiScope("store.cart.manage", "Manage own cart"),
            new ApiScope("store.order.manage", "Place and view own orders"),
            new ApiScope("store.profile.manage", "Manage own profile"),

            // ---- Admin panel scopes (consumed by the Next.js app) ----
            new ApiScope("admin.catalog.manage", "Manage products/catalog"),
            new ApiScope("admin.order.manage", "Manage all orders"),
            new ApiScope("admin.user.manage", "Manage customers/users"),
            new ApiScope("admin.finance.manage", "Manage finance/reports"),

            // ---- Reserved for the future B2B phase, kept minimal on purpose ----
            new ApiScope("b2b.org.manage", "Manage business organization (future use)"),
        };

    // ===================== API Resources =====================
    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("ecommerce_api", "E-Commerce Core API")
            {
                Scopes =
                {
                    "store.catalog.read",
                    "store.cart.manage",
                    "store.order.manage",
                    "store.profile.manage",
                    "admin.catalog.manage",
                    "admin.order.manage",
                    "admin.user.manage",
                    "admin.finance.manage",
                    "b2b.org.manage",
                },
                UserClaims = { "role", "tenant_id", "user_type" }
            }
        };

    // ===================== Clients =====================
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // ========================================================
            // 1) ADMIN PANEL (Next.js) — email + password (+ optional 2FA)
            // ========================================================
            new Client
            {
                ClientId = "ecommerce-admin-panel",
                ClientName = "E-Commerce Admin Panel (Next.js)",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = true,
                ClientSecrets = { new Secret("CHANGE_ME_ADMIN_SECRET".Sha256()) },
                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "email",
                    "roles",
                    "tenant",
                    "user_type",
                    "admin.catalog.manage",
                    "admin.order.manage",
                    "admin.user.manage",
                    "admin.finance.manage",
                    "offline_access"
                },
                RedirectUris = { "https://localhost:3000/api/auth/callback/idsrv" },
                FrontChannelLogoutUri = "https://localhost:3000/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:3000/signout-callback" },
                AllowOfflineAccess = true,
                AllowAccessTokensViaBrowser = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                RequireConsent = false,
                AllowRememberConsent = true,
                AccessTokenLifetime = 3600, // 1 hour
                IdentityTokenLifetime = 300, // 5 minutes
                AbsoluteRefreshTokenLifetime = 2592000, // 30 days
                SlidingRefreshTokenLifetime = 1296000, // 15 days

                // Tells the shared /Account/Login page to render the
                // email + password (+ optional TOTP) view.
                Properties =
                {
                    {
                        ClientPropertyNames.AuthenticationFlow,
                        AuthenticationFlows.Password
                    }
                }
            },


            // ========================================================
            // 2) CUSTOMER WEB SIMULATOR (Next.js) — dev/test stand-in for
            //    the React Native app above, until that app exists. Kept as
            //    its OWN client (not a redirect URI bolted onto #2) so the
            //    real native client's config stays clean and public/PKCE-only.
            //    Safe to delete once the actual RN app is integrated.
            // ========================================================
            new Client
            {
                ClientId = "ecommerce-customer-web-simulator",
                ClientName = "E-Commerce Customer Web Simulator (Next.js)",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = true, // runs server-side (Next.js/NextAuth), safe to hold a secret
                ClientSecrets = { new Secret("CHANGE_ME_CUSTOMER_SIM_SECRET".Sha256()) },
                AllowedScopes =
                {
                    "openid",
                    "profile",
                    "phone",
                    "user_type",
                    "store.catalog.read",
                    "store.cart.manage",
                    "store.order.manage",
                    "store.profile.manage",
                    "offline_access"
                },
                RedirectUris = { "https://localhost:3001/api/auth/callback/idsrv" },
                FrontChannelLogoutUri = "https://localhost:3001/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:3001/signout-callback" },
                AllowOfflineAccess = true,
                RequireConsent = false,
                AllowRememberConsent = true,
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 300,
                AbsoluteRefreshTokenLifetime = 2592000,
                SlidingRefreshTokenLifetime = 1296000,

                // Same "otp" login UI as the real customer client.
                Properties =
                {
                    {
                        ClientPropertyNames.AuthenticationFlow,
                        AuthenticationFlows.Otp
                    }
                }
            }
        };
}