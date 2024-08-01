using Serilog;
using IDPServer.Configs;
using IDPServer.Extensions;


namespace IDPServer;
internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {

        // Bind configuration to AppSettings model
        var appSettings = builder.Services.BuildServiceProvider().GetRequiredService<AppSettings>();


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