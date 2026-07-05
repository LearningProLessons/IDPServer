using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IDPServer.Data;
using IDPServer.Models;
using Serilog;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IDPServer;
 
public sealed class SeedData
{
    private static readonly string[] StaffRoleNames =
    {
        "Admin",

    };

    public static async Task EnsureSeedDataAsync(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var persistedGrantContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        var appContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configurationContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Apply migrations
        await persistedGrantContext.Database.MigrateAsync();
        await configurationContext.Database.MigrateAsync();
        await appContext.Database.MigrateAsync();

        // Seed IdentityServer data (clients/resources/scopes come from Config.cs)
        await SeedConfigurationDataAsync(configurationContext);

        // Seed staff roles + the admin user
        await SeedAdminAsync(roleMgr, userMgr);

     
    }

    private static async Task SeedConfigurationDataAsync(ConfigurationDbContext context)
    {
        if (!await context.Clients.AnyAsync())
        {
            Log.Debug("Clients being populated");
            foreach (var client in Config.Clients.ToList())
            {
                context.Clients.Add(client.ToEntity());
            }

            await context.SaveChangesAsync();
        }
        else
        {
            Log.Debug("Clients already populated");
        }

        if (!await context.IdentityResources.AnyAsync())
        {
            Log.Debug("IdentityResources being populated");
            foreach (var resource in Config.IdentityResources.ToList())
            {
                context.IdentityResources.Add(resource.ToEntity());
            }

            await context.SaveChangesAsync();
        }
        else
        {
            Log.Debug("IdentityResources already populated");
        }

        if (!await context.ApiScopes.AnyAsync())
        {
            Log.Debug("ApiScopes being populated");
            foreach (var scope in Config.ApiScopes.ToList())
            {
                context.ApiScopes.Add(scope.ToEntity());
            }

            await context.SaveChangesAsync();
        }
        else
        {
            Log.Debug("ApiScopes already populated");
        }

        if (!await context.ApiResources.AnyAsync())
        {
            Log.Debug("ApiResources being populated");
            foreach (var resource in Config.ApiResources.ToList())
            {
                context.ApiResources.Add(resource.ToEntity());
            }

            await context.SaveChangesAsync();
        }
        else
        {
            Log.Debug("ApiResources already populated");
        }

        // NOTE: the old demo external OIDC provider (demo.duendesoftware.com) from the
        // previous project was intentionally dropped — it isn't part of this scenario.
        // If/when B2B needs external IdP federation (e.g. a partner's own SSO), add it
        // here as a new OidcProvider/IdentityProvider entry, without touching the rest.
    }

    private static async Task SeedAdminAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        foreach (var roleName in StaffRoleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new ApplicationRole(roleName));
        }

        var adminUser = await userManager.FindByNameAsync("admin@sapegah.com");

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@sapegah.com", // staff log in with email as username
                Email = "admin@sapegah.com",
                EmailConfirmed = true,
                PhoneNumber = "09120000000",
                PhoneNumberConfirmed = true,
                AccessFailedCount = 0,
                TwoFactorEnabled = false, // opt-in: admin can enable TOTP later from their profile
                UserType = UserType.Admin,
                IsBlocked = false,
                ForcePasswordChange = false
            };

            var createUserResult = await userManager.CreateAsync(adminUser, "$Dev1234");

            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }
        }

        foreach (var roleName in StaffRoleNames)
        {
            if (!await userManager.IsInRoleAsync(adminUser, roleName))
                await userManager.AddToRoleAsync(adminUser, roleName);
        }
    }

  
}
