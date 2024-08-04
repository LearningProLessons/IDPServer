using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using IDPServer.Data;
using IDPServer.Models;
using IDPServer.Models.Common;
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

    private static async Task SeedRolesAndUsersAsync(
RoleManager<IdentityRole<int>> roleMgr,
UserManager<ApplicationUser> userMgr,
ApplicationDbContext dbContext) // Inject ApplicationDbContext
    {
        // Ensure roles exist
        var roles = new[] { "admin", "employee", "manager" }; // Define roles
        foreach (var roleName in roles)
        {
            if (await roleMgr.FindByNameAsync(roleName) == null)
            {
                var role = new IdentityRole<int>(roleName);
                var result = await roleMgr.CreateAsync(role);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                Log.Debug($"Role '{roleName}' created");
            }
            else
            {
                Log.Debug($"Role '{roleName}' already exists");
            }
        }

        // Ensure organizations exist using Config
        foreach (var orgName in Config.Organizations)
        {
            if (!await dbContext.Organizations.AnyAsync(o => o.OrganizationName == orgName))
            {
                var organization = new Organization { OrganizationName = orgName };
                dbContext.Organizations.Add(organization);
                await dbContext.SaveChangesAsync();
                Log.Debug($"Organization '{orgName}' created");
            }
            else
            {
                Log.Debug($"Organization '{orgName}' already exists");
            }
        }

        // Example user creation with roles in different organizations
        var users = new List<(string UserName, string Email, string Password, string PhoneNumber, Dictionary<string, string> RolesAndOrganizations)>
    {
        (
            "admin",
            "admin@nill.com",
            "AdminPassword123!",
            "09203216120",
            new Dictionary<string, string> { { "admin", "شرکت پخش پگاه" }, { "employee", "شرکت لینا" } }
        ),
        (
            "user1",
            "user1@example.com",
            "UserPassword123!",
            "09203216121",
            new Dictionary<string, string> { { "employee", "شرکت فیروز" } }
        )
    };

        foreach (var (userName, email, password, phoneNumber, rolesAndOrgs) in users)
        {
            var user = await userMgr.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    AccessFailedCount = 0,
                    PhoneNumber = phoneNumber,
                    TwoFactorEnabled = false
                };

                var result = await userMgr.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Assign roles and organizations
                foreach (var (role, orgName) in rolesAndOrgs)
                {
                    // Ensure role exists
                    var roleExists = await roleMgr.RoleExistsAsync(role);
                    if (!roleExists)
                    {
                        throw new Exception($"Role '{role}' does not exist");
                    }

                    // Assign role to user
                    result = await userMgr.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to assign role '{role}' to user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    // Add claims for organization
                    var organization = await dbContext.Organizations.FirstOrDefaultAsync(o => o.OrganizationName == orgName);
                    if (organization == null)
                    {
                        throw new Exception($"Organization '{orgName}' not found");
                    }

                    var claim = new Claim("organization_claim", orgName); // Updated claim name
                    result = await userMgr.AddClaimAsync(user, claim);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to add organization claim '{orgName}' to user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    // Add scopes based on role
                    if (role == "admin")
                    {
                        await userMgr.AddClaimAsync(user, new Claim("scope", "all.read"));
                        await userMgr.AddClaimAsync(user, new Claim("scope", "all.write"));
                    }
                    else
                    {
                        await userMgr.AddClaimAsync(user, new Claim("scope", "order.read"));
                        await userMgr.AddClaimAsync(user, new Claim("scope", "order.write"));
                    }
                }

                Log.Debug($"User '{userName}' created with roles and organizations");
            }
            else
            {
                Log.Debug($"User '{userName}' already exists");
            }
        }
    }


}