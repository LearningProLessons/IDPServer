namespace IDPServer.Extensions;
public static class CorsExtensions
{
    public static void AddCustomCors(this IServiceCollection services, IEnumerable<string> allowedOrigins)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policyBuilder =>
            {
                policyBuilder.WithOrigins(allowedOrigins.ToArray())
                             .AllowAnyHeader()
                             .AllowCredentials();
            });
        });
    }
}
