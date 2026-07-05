using IDPServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDPServer.Data.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Customers authenticate by phone (OTP), so it needs the same uniqueness
        // guarantee Identity already gives NormalizedUserName/NormalizedEmail.
        // Filtered so multiple staff rows with a NULL phone don't collide.
        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL");

        // Reserved for the B2B phase: speeds up "all users in tenant X" queries
        // once TenantId actually gets populated.
        builder.HasIndex(u => u.TenantId);
    }
}
