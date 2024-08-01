using IDPServer.Models.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace IDPServer.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations", "Sso");

        builder.HasKey(o => o.OrganizationId);

        builder.Property(o => o.OrganizationName)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(o => o.OrganizationName)
               .IsUnique();

        // Add any additional configuration here
    }
}
