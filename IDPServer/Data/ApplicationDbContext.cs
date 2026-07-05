using IDPServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, long>
{
    private readonly string _schemaName;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IConfiguration configuration) : base(options)
    {
        _schemaName = configuration.GetConnectionString("SchemaName") ?? "dbo";
    }

    // Custom tables layered on top of the standard AspNetUsers/AspNetRoles/etc.
    // Kept deliberately minimal — these three are the only e-commerce-specific
    // additions to an otherwise stock ASP.NET Core Identity schema.
    public DbSet<LoginAudit> LoginAudits { get; set; } = null!;
    public DbSet<TrustedDevice> TrustedDevices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(_schemaName);

        // Picks up ApplicationUserConfiguration, LoginAuditConfiguration and
        // TrustedDeviceConfiguration automatically — no need to register them
        // here one by one.
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
