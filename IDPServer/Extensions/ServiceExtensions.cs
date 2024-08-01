using IDPServer.Pages.Admin.ApiScopes;
using IDPServer.Pages.Admin.Clients;
using IDPServer.Pages.Admin.IdentityScopes;
using PortalClientRepository = IDPServer.Pages.Portal.ClientRepository;


namespace IDPServer.Extensions;
public static class ServiceExtensions
{
    public static void AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<PortalClientRepository>();
        services.AddTransient<ClientRepository>();
        services.AddTransient<IdentityScopeRepository>();
        services.AddTransient<ApiScopeRepository>();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(10);
            options.Cookie.HttpOnly = false;
            options.Cookie.IsEssential = true;
        });
    }
}
