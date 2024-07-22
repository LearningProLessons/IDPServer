using IdentityModel;
using IDPServer.Data;
using IDPServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace IDPServer;

public static class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Check and create 'admin' role if it does not exist
        var adminRole = roleMgr.FindByNameAsync("admin").Result;
        if (adminRole == null)
        {
            adminRole = new IdentityRole("admin");
            var result = roleMgr.CreateAsync(adminRole).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("admin role created");
        }
        else
        {
            Log.Debug("admin role already exists");
        }

        // Check and create 'admin' user if it does not exist
        var admin = userMgr.FindByNameAsync("admin").Result;
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                EmailConfirmed = true
            };
            var result = userMgr.CreateAsync(admin, "Sap@admin1234").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddToRoleAsync(admin, "admin").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(admin,
            [
                new Claim(JwtClaimTypes.Name, "Admin User"),
                new Claim(JwtClaimTypes.GivenName, "Admin"),
                new Claim(JwtClaimTypes.FamilyName, "User"),
                new Claim(JwtClaimTypes.WebSite, "https://www.sapegah.com"),
            ]).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("admin user created");
        }
        else
        {
            Log.Debug("admin user already exists");
        }
    }
}
