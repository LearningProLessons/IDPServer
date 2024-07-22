using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using IDPServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IDPServer;

public class SeedData
{
    public static async Task EnsureSeedDataAsync(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var persistedGrantContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
        var configurationContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Apply migrations
        await persistedGrantContext.Database.MigrateAsync();
        await configurationContext.Database.MigrateAsync();

        // Seed IdentityServer data
        await SeedConfigurationDataAsync(configurationContext);

        // Seed roles and users
        await SeedRolesAndUsersAsync(roleMgr, userMgr);
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

        if (!await context.IdentityProviders.AnyAsync())
        {
            Log.Debug("OIDC IdentityProviders being populated");
            context.IdentityProviders.Add(new OidcProvider
            {
                Scheme = "demoidsrv",
                DisplayName = "IdentityServer",
                Authority = "https://demo.duendesoftware.com",
                ClientId = "login",
            }.ToEntity());
            await context.SaveChangesAsync();
        }
        else
        {
            Log.Debug("OIDC IdentityProviders already populated");
        }
    }

    private static async Task SeedRolesAndUsersAsync(RoleManager<IdentityRole> roleMgr, UserManager<ApplicationUser> userMgr)
    {
        // Check and create 'admin' role if it does not exist
        var adminRole = await roleMgr.FindByNameAsync("admin");
        if (adminRole == null)
        {
            adminRole = new IdentityRole("admin");
            var result = await roleMgr.CreateAsync(adminRole);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create role 'admin': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            Log.Debug("Admin role created");
        }
        else
        {
            Log.Debug("Admin role already exists");
        }

        // Check and create 'admin' user if it does not exist
        var admin = await userMgr.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                EmailConfirmed = true,
                FirstName = "Admin",  // Ensure these fields exist in ApplicationUser
                LastName = "User",
                // Add other properties as needed
            };

            var result = await userMgr.CreateAsync(admin, "Sap@admin1234");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create user 'admin': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            result = await userMgr.AddToRoleAsync(admin, "admin");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to assign role to user 'admin': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Name, "Admin User"),
                new Claim(JwtClaimTypes.GivenName, "Admin"),
                new Claim(JwtClaimTypes.FamilyName, "User"),
                new Claim(JwtClaimTypes.WebSite, "https://www.sapegah.com")
            };

            result = await userMgr.AddClaimsAsync(admin, claims);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to add claims to user 'admin': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            Log.Debug("Admin user created");
        }
        else
        {
            Log.Debug("Admin user already exists");
        }
    }
}
