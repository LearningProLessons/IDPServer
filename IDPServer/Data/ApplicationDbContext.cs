using IDPServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace IDPServer.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public virtual DbSet<Province> Provinces { get; set; }
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<AddressUser> AddressUsers { get; set; }
    public virtual DbSet<City> Cities { get; set; }
    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }
    public virtual DbSet<CompanyAccount> CompanyAccounts { get; set; }
    public virtual DbSet<CompanyCustomer> CompanyCustomers { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        #region BaseConfig
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        // Configure schema
        builder.HasDefaultSchema("Sso");
        #endregion


        // Apply configurations from the current assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
