using Serilog;
 
using IDPServer.Extensions;
using Microsoft.Extensions.Options;


namespace IDPServer;
internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // Bind configuration
        builder.Services.Configure<AppSettings>(builder.Configuration);

        // Build service provider and get AppSettings
        var appSettings = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<AppSettings>>().Value;

    

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




        builder.Services.AddCustomServices();
        builder.Services.AddCustomDbContext(appSettings.ConnectionStrings.DefaultConnection);
        builder.Services.AddCustomIdentityServer(appSettings.ConnectionStrings.DefaultConnection);
        builder.Services.AddCustomCors(appSettings.Cors.AllowedOrigins);
        builder.Services.AddCustomAuthentication();







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

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();


      
        app.MapRazorPages().RequireAuthorization();
        return app;
    }
}