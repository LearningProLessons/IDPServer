using IDPServer.Data;
using IDPServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Extensions;

public static class IdentityServerExtensions
{
    public static void AddCustomIdentityServer(this IServiceCollection services, string connectionString)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = false;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 2;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddIdentityServer(options =>
        {
            options.ServerSideSessions.UserDisplayNameClaimType = "name";
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
            options.EmitStaticAudienceClaim = true;
        })
        .AddServerSideSessions()
        .AddConfigurationStore(options =>
        {
            options.ConfigureDbContext = b =>
                b.UseSqlServer(connectionString,
                    dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
            options.DefaultSchema = "Sso";
        })
        .AddOperationalStore(options =>
        {
            options.ConfigureDbContext = b =>
                b.UseSqlServer(connectionString,
                    dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
            options.DefaultSchema = "Sso";
            options.TokenCleanupInterval = 3600; // Cleanup interval in seconds
        })
        .AddDeveloperSigningCredential()
        .AddConfigurationStoreCache()
        .AddInMemoryClients(Config.Clients)
        //.AddInMemoryApiScopes(Config.ApiScopes)
        //.AddInMemoryApiResources(Config.ApiResources)
        .AddAspNetIdentity<ApplicationUser>();
    }
}

