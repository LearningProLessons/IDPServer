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

            // IMPORTANT: this used to only cover "/Account/Create". Since
            // ConfigurePipeline() calls app.MapRazorPages().RequireAuthorization(),
            // that left /Account/Login itself behind the same auth requirement —
            // an unauthenticated visitor hitting Login would get redirected back
            // to Login forever. The whole Account folder (Login, Create, Logout,
            // AccessDenied) needs to be reachable anonymously.
            options.Conventions.AllowAnonymousToFolder("/Account");
        });


        services.AddTransient<PortalClientRepository>();
        services.AddTransient<ClientRepository>();
        services.AddTransient<IdentityScopeRepository>();
        services.AddTransient<ApiScopeRepository>();

        // Backs the OTP resend-cooldown / attempt counter in OtpService — fine as
        // in-memory for now (single instance); swap for a distributed cache
        // (e.g. Redis) if/when this runs behind more than one instance.
        services.AddMemoryCache();
        services.AddSingleton<ISmsSender, ConsoleSmsSender>();
        services.AddScoped<IOtpService, OtpService>();

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

        // Registered AFTER AddAspNetIdentity<ApplicationUser>() on purpose: that call
        // registers Duende's own default IProfileService for ASP.NET Identity, and in
        // the .NET DI container the LAST registration for a service type wins. Adding
        // ours first (as it was before) risks silently being shadowed by the default
        // one, which would drop the "role"/"tenant_id"/"user_type" claims entirely.
        services.AddScoped<IProfileService, ProfileService>();

        // IMPORTANT FIX: this used to be
        //   services.AddAuthentication("Cookies").AddCookie("Cookies", ...).AddGoogle(...)
        // which explicitly set "Cookies" as the DEFAULT authentication scheme.
        // But SignInManager<ApplicationUser>.SignInAsync() always writes to
        // IdentityConstants.ApplicationScheme ("Identity.Application"), never to
        // "Cookies" — so nothing was actually reading/writing that scheme, while
        // the real Identity cookie risked NOT being treated as the default. That
        // could make IdentityServer's own login check think the user was never
        // signed in even right after a successful Login.cshtml.cs post. Removed;
        // AddIdentity(...) above already wires up sensible authentication defaults.
        services.AddAuthorization();

        services.AddAuthentication()
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