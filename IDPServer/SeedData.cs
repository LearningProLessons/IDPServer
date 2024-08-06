using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using IDPServer.Data;
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
        var appContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configurationContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
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

        //if (!await context.ApiResources.AnyAsync())
        //{
        //    Log.Debug("ApiResources being populated");
        //    foreach (var scope in Config.ApiResources.ToList())
        //    {
        //        context.ApiResources.Add(scope.ToEntity());
        //    }
        //    await context.SaveChangesAsync();
        //}
        //else
        //{
        //    Log.Debug("ApiResources already populated");
        //}

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


    private static async Task SeedRolesAndUsersAsync(RoleManager<IdentityRole<int>> roleMgr, UserManager<ApplicationUser> userMgr)
    {
        // Seed roles
        var roles = new[] { "Admin", "User", "OrganizationManager" };
        foreach (var role in roles)
        {
            if (!await roleMgr.RoleExistsAsync(role))
            {
                await roleMgr.CreateAsync(new IdentityRole<int>(role));
            }
        }

        // Seed admin user
        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@sapegah.com",
            EmailConfirmed = true,
            AccessFailedCount = 0,
            PhoneNumber = "09120000000",
            TwoFactorEnabled = false,
            NormalizedUserName = "SAP",
            PhoneNumberConfirmed = true,
        };

        // Create the admin user if it doesn't already exist
        var result = await userMgr.CreateAsync(admin, "Sap@admin1234");
        if (result.Succeeded)
        {
            // Assign roles to the admin user
            await userMgr.AddToRoleAsync(admin, "Admin");

            // Add organization claim
            await userMgr.AddClaimAsync(admin, new Claim("organizationId", "12")); // Replace with the actual organization ID

            // If you have more organization-related claims, add them here as well
            await userMgr.AddClaimAsync(admin, new Claim("organizationName", "Pegah")); // Optional claim
        }
        else
        {
            // Handle user creation failure if necessary
            throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }




}