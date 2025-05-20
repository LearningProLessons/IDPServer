using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IDPServer.Data;
using IDPServer.Models;
using Serilog;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace IDPServer;

public sealed class SeedData
{
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

        // Seed IdentityServer data
        await SeedConfigurationDataAsync(configurationContext);


        // Seed roles and users
        await SeedRolesAndUsersAsync(roleMgr, userMgr, appContext);
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


    private static async Task SeedRolesAndUsersAsync(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        var roleNames = new[] { "Admin", "CharitySuperAdmin", "CampaignManager", "FinanceManager" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new ApplicationRole(roleName));
        }

        if (!await dbContext.Branches.AnyAsync())
        {
            var branches = new[]
            {
                new Branch { Name = "Tehran" },
                new Branch { Name = "Semnan" }, 
                new Branch { Name = "Isfahhan" },
                new Branch { Name = "Ahvaz" }
            };

            await dbContext.Branches.AddRangeAsync(branches);
            await dbContext.SaveChangesAsync();
        }

        var adminUser = await userManager.FindByNameAsync("admin");

        if (adminUser == null)
        {
            var branch = await dbContext.Branches.FirstAsync();

            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@sapegah.com",
                EmailConfirmed = true,
                AccessFailedCount = 0,
                PhoneNumber = "09120000000",
                TwoFactorEnabled = false,
                NormalizedUserName = "SAP",
                PhoneNumberConfirmed = true,
                BranchId = branch.Id
            };

            var createUserResult = await userManager.CreateAsync(adminUser, "$Dev1234");

            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }
        }

        foreach (var roleName in roleNames)
        {
            if (!await userManager.IsInRoleAsync(adminUser, roleName))
                await userManager.AddToRoleAsync(adminUser, roleName);
        }
    }
}