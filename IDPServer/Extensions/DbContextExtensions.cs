using IDPServer.Data;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Extensions;
public static class DbContextExtensions
{
    public static void AddCustomDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName)));
    }
}
