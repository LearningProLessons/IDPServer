using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using IDPServer.Data;
using Serilog;
using IDPServer.Models;
using IDPServer.Pages.Admin.ApiScopes;
using IDPServer.Pages.Admin.Clients;
using IDPServer.Pages.Admin.IdentityScopes;
using IDPServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalClientRepository = IDPServer.Pages.Portal.ClientRepository;

namespace IDPServer;

internal static class HostingExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var schemaName = configuration.GetConnectionString("SchemaName");


        services.AddRazorPages(options =>
        {
            // options.Conventions.AuthorizePage("/Contact");
            // options.Conventions.AuthorizeFolder("/Dashboard");
            //options.Conventions.AllowAnonymousToPage("/Private/PublicPage");
            options.Conventions.AllowAnonymousToFolder("/Account/Create");
        });


        services.AddTransient<PortalClientRepository>();
        services.AddTransient<ClientRepository>();
        services.AddTransient<IdentityScopeRepository>();
        services.AddTransient<ApiScopeRepository>();

        services.AddScoped<IProfileService, ProfileService>();


        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(10);
            options.Cookie.IsEssential = true;
        });
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString,
                dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName)));
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 2;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;

                // options.SignIn.RequireConfirmedEmail = true;
                // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                // options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddIdentityServer(options =>
            {
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;


                // options.ServerSideSessions.UserDisplayNameClaimType = "name";
                // options.EmitStaticAudienceClaim = true;
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddServerSideSessions()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString,
                        dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
                options.DefaultSchema = schemaName;
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString,
                        dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
                options.DefaultSchema = schemaName;
                options.EnableTokenCleanup = false;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddConfigurationStoreCache()
            .AddDeveloperSigningCredential();


        services.AddAuthorization(options =>
            {
                options.AddPolicy("CharitySuperAdminPolicy", policy =>
                    policy.RequireClaim("role", "CharitySuperAdmin")
                        .RequireClaim("tenant_id"));

                options.AddPolicy("CampaignManagerPolicy", policy =>
                    policy.RequireClaim("role", "CampaignManager")
                        .RequireClaim("tenant_id"));

                options.AddPolicy("FinanceManagerPolicy", policy =>
                    policy.RequireClaim("role", "FinanceManager")
                        .RequireClaim("tenant_id"));
            })
            .AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                options.Cookie.Name = "MyApp.Cookie";
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
            })
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });
    }

    public static void ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSession();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();


        app.MapRazorPages().RequireAuthorization();
    }
}