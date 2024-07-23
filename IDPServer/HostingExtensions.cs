using Duende.IdentityServer;
using IDPServer.Data;
using IDPServer.Models;
using IDPServer.Pages.Admin.ApiScopes;
using IDPServer.Pages.Admin.IdentityScopes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IDPServer;
internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        #region ProtectRoute
        // Add services to the container.
        builder.Services.AddRazorPages(options =>
        {
            // options.Conventions.AuthorizePage("/Contact");
            // options.Conventions.AuthorizeFolder("/Dashboard");
            //options.Conventions.AllowAnonymousToPage("/Private/PublicPage");
            options.Conventions.AllowAnonymousToFolder("/Account/Create");
        });

        #endregion

        #region Dependency Injections  
        builder.Services.AddTransient<Pages.Portal.ClientRepository>();
        builder.Services.AddTransient<Pages.Admin.Clients.ClientRepository>();
        builder.Services.AddTransient<IdentityScopeRepository>();
        builder.Services.AddTransient<ApiScopeRepository>();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(10);
            options.Cookie.HttpOnly = false;
            options.Cookie.IsEssential = true;
        });
        #endregion

        #region Required Variables
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        #endregion

        #region DbContext DI
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName)));
        #endregion



        #region IdentityServer Configurations

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            //options.SignIn.RequireConfirmedEmail = true; 

            //User validator
            options.User.RequireUniqueEmail = false;

            //Password Validator
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

      builder.Services
     .AddIdentityServer(options =>
     {
         options.ServerSideSessions.UserDisplayNameClaimType = "name";

         options.Events.RaiseErrorEvents = true;
         options.Events.RaiseInformationEvents = true;
         options.Events.RaiseFailureEvents = true;
         options.Events.RaiseSuccessEvents = true;

         // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
         options.EmitStaticAudienceClaim = true;

     })
     .AddServerSideSessions()
     .AddConfigurationStore(options =>
     {
         options.ConfigureDbContext = b =>
             b.UseSqlServer(connectionString,
                 dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
         options.DefaultSchema = "Sso";
     }).AddOperationalStore(options =>
     {
         options.ConfigureDbContext = b =>
             b.UseSqlServer(connectionString,
                 dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
         options.DefaultSchema = "Sso";

         // Set token expiration times in operational store options
         options.TokenCleanupInterval = 3600; // Cleanup interval in seconds
     })
     .AddDeveloperSigningCredential()
     // this is something you will want in production to reduce load on and requests to the DB
     .AddConfigurationStoreCache()
     //
     // this adds the operational data from DB (codes, tokens, consents)
    .AddInMemoryClients(Config.Clients)
    .AddAspNetIdentity<ApplicationUser>();


        #endregion



        #region CORS config
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!;

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
            {
                policyBuilder.WithOrigins(allowedOrigins)
                             .AllowAnyHeader()
                             .AllowCredentials();
            });
        });
        #endregion



        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.UseSession();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}
