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
    ApplicationDbContext dbContext)
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

        // Ensure companies exist using Config
        foreach (var orgName in Config.Companies)
        {
            if (!await dbContext.Companies.AnyAsync(o => o.Name == orgName))
            {
                var company = new Company { Name = orgName };
                dbContext.Companies.Add(company);
                await dbContext.SaveChangesAsync();
                Log.Debug($"Company '{orgName}' created");
            }
            else
            {
                Log.Debug($"Company '{orgName}' already exists");
            }
        }

        // Get company IDs for easier access
        var organizationIds = await dbContext.Companies
            .ToDictionaryAsync(o => o.Name, o => o.Id);

        // Example user creation with roles in different companies
        var users = new List<(string UserName, string Email, string Password, string PhoneNumber, Dictionary<string, int> RolesAndOrganizations)>
    {
        (
            "admin",
            "admin@nill.com",
            "AdminPassword123!",
            "09203216120",
            new Dictionary<string, int> { { "admin", organizationIds["شرکت پخش پگاه"] }, { "employee", organizationIds["شرکت لینا"] } }
        ),
        (
            "user1",
            "user1@example.com",
            "UserPassword123!",
            "09203216121",
            new Dictionary<string, int> { { "employee", organizationIds["شرکت فیروز"] } }
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

                // Assign roles and companies
                foreach (var (role, orgId) in rolesAndOrgs)
                {
                    // Ensure role exists
                    if (!await roleMgr.RoleExistsAsync(role))
                    {
                        throw new Exception($"Role '{role}' does not exist");
                    }

                    // Assign role to user
                    result = await userMgr.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to assign role '{role}' to user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    // Add claims for company using ID
                    var company = await dbContext.Companies.FindAsync(orgId);
                    if (company == null)
                    {
                        throw new Exception($"Company with ID '{orgId}' not found");
                    }

                    var claim = new Claim("organization_id", orgId.ToString());
                    result = await userMgr.AddClaimAsync(user, claim);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to add company claim '{orgId}' to user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    // Add claims for roles
                    var roleClaim = new Claim(JwtClaimTypes.Role, role);
                    result = await userMgr.AddClaimAsync(user, roleClaim);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to add role claim '{role}' to user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    // Add user name claim
                    var nameClaim = new Claim(JwtClaimTypes.Name, userName);
                    result = await userMgr.AddClaimAsync(user, nameClaim);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to add name claim '{userName}' to user '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    // Add scopes based on role
                    var scopes = role == "admin"
                        ? new[] { "all.read", "all.write" }
                        : new[] { "orders.read", "orders.write" };

                    foreach (var scope in scopes)
                    {
                        await userMgr.AddClaimAsync(user, new Claim("scope", scope));
                    }
                }

                Log.Debug($"User '{userName}' created with roles and companies");
            }
            else
            {
                Log.Debug($"User '{userName}' already exists");
            }
        }
    }

}